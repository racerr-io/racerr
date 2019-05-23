using Mirror;
using Racerr.Track;
using Racerr.UX.HUD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Racerr Race Session Manager. Manages the current race, cars and players
    /// on the current dedicated server.
    /// </summary>
    public class RacerrRaceSessionManager : NetworkBehaviour
    {
        // Server and client properties
        public static RacerrRaceSessionManager Singleton;
        [SyncVar] bool isCurrentlyRacing;
        public bool IsCurrentlyRacing => isCurrentlyRacing;
        [SyncVar] float raceStartTime;
        public float RaceStartTime => raceStartTime;

        // Server only properties
        [SerializeField] int raceTimerSeconds = 5;
        [SerializeField] int raceTimerSecondsSinglePlayer = 20;
        List<Player> playersOnServer = new List<Player>();
        List<Player> playersInRace = new List<Player>();
        List<Player> finishedPlayers = new List<Player>();
        GameObject[] checkpointsInRace = null;
        bool timerActive = false;
        public IReadOnlyCollection<Player> PlayersOnServer => playersOnServer;
        public IReadOnlyCollection<Player> ReadyPlayers => playersOnServer.Where(p => p.IsReady).ToArray();
        public IReadOnlyCollection<Player> PlayersInRace => playersInRace;
        public IReadOnlyCollection<Player> FinishedPlayers => finishedPlayers;
        public IEnumerable<Player> PlayersInRaceOrdered
        {
            get
            {
                return PlayersInRace
                    .OrderBy(player => player.PositionInfo.FinishingTime)
                    .OrderByDescending(player => player.PositionInfo.Checkpoints.Count)
                    .ThenBy(player =>
                    {
                        Vector3? currCarPosition = player.Car?.transform.position;
                        if (currCarPosition == null || checkpointsInRace == null)
                        {
                            // For some reason the player has no car or the race hasn't started,
                            // so let's just be safe rather than crash.
                            return float.PositiveInfinity;
                        }

                        // checkpointsInRace is sorted in the order of the checkpoints in the race,
                        // so to grab the next checkpoint for this car we use the checkpoint count for this player as an index.
                        int nextCheckpoint = player.PositionInfo.Checkpoints.Count;
                        Vector3 nextCheckpointPosition = checkpointsInRace[nextCheckpoint].transform.position;
                        return Vector3.Distance(currCarPosition.Value, nextCheckpointPosition);
                    });
            }
        }

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one race session manager is created
        /// in the scene.
        /// </summary>
        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Debug.LogError("You can only have one Racerr Race Session Manager in the scene. The extra one has been destroyed.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Called every so often to update the current state of the race on this server.
        /// </summary>
        void LateUpdate()
        {
            if (isServer)
            {
                if (playersOnServer.Any(p => p.IsReady) && !isCurrentlyRacing && !timerActive)
                {
                    timerActive = true;
                    int seconds = ReadyPlayers.Count > 1 ? raceTimerSeconds : raceTimerSecondsSinglePlayer;
                    FindObjectOfType<RaceTimer>().StartTimer(seconds);
                }
                else if (isCurrentlyRacing && (playersInRace.Count == 0 || finishedPlayers.Count == playersInRace.Count))
                {
                    EndRace();
                }
            }
        }

        /// <summary>
        /// Add a new player to the Race Session Manager.
        /// </summary>
        /// <param name="playerGameObject">Player Game Object.</param>
        [Server]
        public void AddNewPlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            playersOnServer.Add(player);
        }

        /// <summary>
        /// Remove an existing player from the Race Session Manager.
        /// </summary>
        /// <param name="playerGameObject">Player Game Object.</param>
        [Server]
        public void RemovePlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            playersOnServer.Remove(player);
            playersInRace.Remove(player);
            finishedPlayers.Remove(player);
        }

        /// <summary>
        /// Start a new race, provided one is not currently occuring.
        /// </summary>
        [Server]
        public void StartRace()
        {
            StartCoroutine(StartRaceCore());
        }

        /// <summary>
        /// Coroutine for race starting, since we need to wait for the track 
        /// to be generated before executing more code (simulating a semaphore).
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        [Server]
        IEnumerator StartRaceCore()
        {
            if (!isCurrentlyRacing)
            {
                TrackGeneratorCommon.Singleton.GenerateIfRequired();
                while (!TrackGeneratorCommon.Singleton.IsTrackGenerated) yield return null;

                // Create a list of all checkpoints in race using the Generated track pices.
                // We assume all track pieces are either Checkpoint or FinishingLineCheckpoint.
                checkpointsInRace = TrackGeneratorCommon.Singleton.GeneratedTrackPieces.Select(trackPiece => 
                {
                    GameObject result = trackPiece.transform.Find(TrackPieceComponent.Checkpoint)?.gameObject;

                    if (result == null)
                    {
                        // Special case for the finishing line, it has a different label.
                        result = trackPiece.transform.Find(TrackPieceComponent.FinishLineCheckpoint).gameObject;
                    }

                    return result;
                }).ToArray();

                timerActive = false;
                Vector3 currPosition = new Vector3(0, 1, 10);
                playersInRace.AddRange(ReadyPlayers);

                foreach (Player player in PlayersInRace)
                {
                    player.CreateCarForPlayer(currPosition);
                    player.PositionInfo = new PlayerPositionInfo();
                    currPosition += new Vector3(0, 0, 10);
                }

                raceStartTime = Time.time;
                isCurrentlyRacing = true;
            }
        }

        /// <summary>
        /// End the currently running race.
        /// </summary>
        [Server]
        public void EndRace()
        {
            isCurrentlyRacing = false;
            playersInRace.ForEach(p => {
                p.DestroyPlayersCar();
                p.PositionInfo = null;
            });
            playersInRace.Clear();
            finishedPlayers.Clear();
            checkpointsInRace = null;
            TrackGeneratorCommon.Singleton.DestroyIfRequired();
        }

        /// <summary>
        /// Server side only - call this function once the car finishes the race so that
        /// their car is removed and they are marked as finished.
        /// </summary>
        /// <param name="player">The player that finished.</param>
        [Server]
        public void NotifyPlayerFinished(Player player)
        {
            finishedPlayers.Add(player);
            player.PositionInfo.MarkAsFinished();
            player.DestroyPlayersCar();
        }

        /// <summary>
        /// Server side only - call this function when the car moves through a checkpoint.
        /// Adds checkpoint to a set of checkpoints the player has passed through so that
        /// we can calculate their position.
        /// Additionally, check if the player has actually finished the race.
        /// </summary>
        /// <param name="player">The player that passed through.</param>
        /// <param name="checkpoint">The checkpoint the player hit.</param>
        [Server]
        public void NotifyPlayerPassedThroughCheckpoint(Player player, GameObject checkpoint)
        {
            player.PositionInfo.Checkpoints.Add(checkpoint);

            if (checkpoint.name == TrackPieceComponent.FinishLineCheckpoint)
            {
                NotifyPlayerFinished(player);
            }
        }
    }
}