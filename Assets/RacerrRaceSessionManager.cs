using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.RaceSessionManager
{
    public class RacerrRaceSessionManager : NetworkBehaviour
    {
        public static RacerrRaceSessionManager Singleton;
        List<Player> Players { get; } = new List<Player>();
        List<Player> PlayersInRace { get; } = new List<Player>();
        List<Player> FinishedPlayers { get; } = new List<Player>();
        bool IsCurrentlyRacing { get; set; } = false;

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
        /// Called every physics tick to manage the current state of the race on this server.
        /// </summary>
        void FixedUpdate()
        {
            if (Players.Count > 0 && !IsCurrentlyRacing)
            {
                StartRace();
            }
            else if (IsCurrentlyRacing && (Players.Count == 0 || FinishedPlayers.Count == PlayersInRace.Count))
            {
                EndRace();
            }
        }

        public void AddNewPlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            Players.Add(player);
        }

        public void RemovePlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            Players.Remove(player);
            PlayersInRace.Remove(player);
            FinishedPlayers.Remove(player);
        }

        public void StartRace()
        {
            TrackGeneratorCommon.Singleton.GenerateIfRequired();
            Vector3 currPosition = Vector3.zero;
            if (!IsCurrentlyRacing)
            {
                IsCurrentlyRacing = true;
                PlayersInRace.AddRange(Players);
                foreach (Player player in Players)
                {
                    player.CreateCarForPlayer(NetworkManager.singleton.playerPrefab, currPosition);
                    currPosition += new Vector3(0, 0, 10);
                }
            }
        }

        public void EndRace()
        {
            IsCurrentlyRacing = false;
            PlayersInRace.ForEach(p => p.DestroyPlayersCar());
            PlayersInRace.RemoveAll(_ => true);
            FinishedPlayers.RemoveAll(_ => true);
            TrackGeneratorCommon.Singleton.DestroyIfRequired();
        }

        [Server]
        public void NotifyPlayerFinished(Player player)
        {
            FinishedPlayers.Add(player);
            player.DestroyPlayersCar();
        }
    }
}

