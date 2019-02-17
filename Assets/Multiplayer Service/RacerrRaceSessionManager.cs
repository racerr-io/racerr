using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using Racerr.UX.HUD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.RaceSessionManager
{
    /// <summary>
    /// Racerr Race Session Manager. Manages the current race, cars and players
    /// on the current dedicated server.
    /// </summary>
    public class RacerrRaceSessionManager : NetworkBehaviour
    {
        public static RacerrRaceSessionManager Singleton;
        public IReadOnlyCollection<Player> PlayersOnServer => playersOnServer;
        public IReadOnlyCollection<Player> ReadyPlayers => playersOnServer.Where(p => p.IsReady).ToArray();
        public IReadOnlyCollection<Player> PlayersInRace => playersInRace;
        public IReadOnlyCollection<Player> FinishedPlayers => finishedPlayers;
        public bool IsCurrentlyRacing => isCurrentlyRacing;

        [SyncVar] bool isCurrentlyRacing;
        [SerializeField] int raceTimerSeconds = 5;
        [SerializeField] int raceTimerSecondsSinglePlayer = 20;
        List<Player> playersOnServer = new List<Player>();
        List<Player> playersInRace = new List<Player>();
        List<Player> finishedPlayers = new List<Player>();
        bool timerActive = false;

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

        void Start()
        {
            if (isServer)
            {
                InvokeRepeating("UpdateRaceStatus", 0, 5f);
            }
        }

        /// <summary>
        /// Called every so often to update the current state of the race on this server.
        /// </summary>
        [Server]
        void UpdateRaceStatus()
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

        [Server]
        IEnumerator StartRaceCore()
        {
            if (!isCurrentlyRacing)
            {
                TrackGeneratorCommon.Singleton.GenerateIfRequired();
                while (!TrackGeneratorCommon.Singleton.IsTrackGenerated) yield return null;

                timerActive = false;
                Vector3 currPosition = new Vector3(0, 1, 0);
                isCurrentlyRacing = true;
                playersInRace.AddRange(ReadyPlayers);

                foreach (Player player in ReadyPlayers)
                {
                    player.CreateCarForPlayer(NetworkManager.singleton.playerPrefab, currPosition);
                    currPosition += new Vector3(0, 0, 10);
                }
            }
        }

        /// <summary>
        /// End the race.
        /// </summary>
        [Server]
        public void EndRace()
        {
            isCurrentlyRacing = false;
            playersInRace.ForEach(p => p.DestroyPlayersCar());
            playersInRace.RemoveAll(_ => true);
            finishedPlayers.RemoveAll(_ => true);
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
    }
}

