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

        // Server only properties
        [SerializeField] int raceTimerSeconds = 5;
        [SerializeField] int raceTimerSecondsSinglePlayer = 20;
        List<Player> playersOnServer = new List<Player>();
        List<Player> playersInRace = new List<Player>();
        List<Player> finishedPlayers = new List<Player>();
        bool timerActive = false;
        public IReadOnlyCollection<Player> PlayersOnServer => playersOnServer;
        public IReadOnlyCollection<Player> ReadyPlayers => playersOnServer.Where(p => p.IsReady).ToArray();
        public IReadOnlyCollection<Player> PlayersInRace => playersInRace;
        public IReadOnlyCollection<Player> FinishedPlayers => finishedPlayers;

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

                checkPoints = TrackGeneratorCommon.Singleton.GeneratedTrackPieces.Select(piece => 
                {
                    GameObject result = piece.transform.Find(TrackPieceComponent.Checkpoint)?.gameObject;

                    if (result == null)
                    {
                        result = piece.transform.Find(TrackPieceComponent.FinishLineCheckpoint).gameObject;
                    }

                    return result;
                }).ToArray();

                timerActive = false;
                Vector3 currPosition = new Vector3(0, 1, 10);
                isCurrentlyRacing = true;
                playersInRace.AddRange(ReadyPlayers);

                foreach (Player player in ReadyPlayers)
                {
                    player.CreateCarForPlayer(currPosition);
                    playerPositionInfos[player] = new PositionInfo();
                    currPosition += new Vector3(0, 0, 10);
                }
            }
        }

        /// <summary>
        /// End the currently running race.
        /// </summary>
        [Server]
        public void EndRace()
        {
            isCurrentlyRacing = false;
            playersInRace.ForEach(p => p.DestroyPlayersCar());
            playersInRace.Clear();
            finishedPlayers.Clear();
            playerPositionInfos.Clear();
            checkPoints = new GameObject[0];
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
            player.DestroyPlayersCar();
        }

        public IEnumerable<KeyValuePair<Player, PositionInfo>> PlayerOrderedPositions
        {
            get
            {
                return playerPositionInfos
                    .OrderByDescending(l => l.Value.checkpoints.Count)
                    .ThenByDescending(l => l.Value.CalculateDistanceToNextCheckpoint(l.Key.Car?.transform.position, checkPoints));
            }
        }

        GameObject[] checkPoints = new GameObject[0];
        Dictionary<Player, PositionInfo> playerPositionInfos = new Dictionary<Player, PositionInfo>();

        public void NotifyPlayerPassedThroughCheckpoint(Player player, GameObject checkpoint)
        {
            playerPositionInfos[player].AddCheckPoint(checkpoint);
        }

        public class PositionInfo
        {
            public HashSet<GameObject> checkpoints = new HashSet<GameObject>();

            public void AddCheckPoint(GameObject checkpoint)
            {
                if (!checkpoints.Contains(checkpoint))
                {
                    checkpoints.Add(checkpoint);
                }
            }

            public float CalculateDistanceToNextCheckpoint(Vector3? position, GameObject[] checkPoints)
            {
                return Vector3.Distance(position.GetValueOrDefault(Vector3.positiveInfinity), checkPoints[checkpoints.Count].transform.position);
            }
        }
    }
}

