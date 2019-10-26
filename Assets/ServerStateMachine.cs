using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public sealed class ServerStateMachine : NetworkBehaviour, IStateMachine
    {
        public static ServerStateMachine Singleton;

        [SyncVar] StateEnum stateType;
        public StateEnum StateType
        {
            get => stateType;
            private set => stateType = value;
        }

        NetworkedState currentState;

        readonly List<Player> playersInServer = new List<Player>();
        public IReadOnlyCollection<Player> PlayersInServer => playersInServer;
        public IReadOnlyCollection<Player> ReadyPlayers => playersInServer.Where(p => p.IsReady).ToArray();

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one Server State Machine is in the scene.
        /// </summary>
        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Debug.LogError("You can only have one Server State Machine in the scene. The extra one has been destroyed.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Entrypoint into Server State Machine. Called after unity initialises all scripts.
        /// Defaults to an Idle server state.
        /// </summary>
        void Start()
        {
            if (isServer)
            {
                ChangeState(StateEnum.ServerIdle);
            }
            else
            {
                enabled = false;
            }
        }

        /// <summary>
        /// Changes the state of the Server State Machine.
        /// Intended to be PROTECTED - only the Server States should be able to call this from their encapsulated transition methods.
        /// Changes the internal state of the Server State Machine based on the given state type Enum.
        /// </summary>
        /// <param name="stateType">The new state type to be changed to.</param>
        /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
        [Server]
        public void ChangeState(StateEnum stateType, object optionalData = null)
        {
            if (currentState != null)
            {
                // Only time when the current state will be null is when the server starts.
                currentState.Exit();
                currentState.enabled = false;
            }

            try
            {
                switch (stateType)
                {
                    case StateEnum.Intermission: currentState = GetComponent<ServerIntermissionState>(); break;
                    case StateEnum.Race: currentState = GetComponent<ServerRaceState>(); break;
                    case StateEnum.ServerIdle: currentState = GetComponent<ServerIdleState>(); break;
                    default: throw new InvalidOperationException("Invalid Server ChangeState attempt: " + stateType.ToString());
                }
                StateType = stateType;

                currentState.enabled = true;
                currentState.Enter(optionalData);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Add a new player to the Server State.
        /// </summary>
        /// <param name="playerGameObject">Player Game Object.</param>
        [Server]
        public void AddNewPlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            playersInServer.Add(player);
        }

        /// <summary>
        /// Remove an existing player from the Server State, and potential Race Session.
        /// </summary>
        /// <param name="playerGameObject">Player Game Object.</param>
        [Server]
        public void RemovePlayer(GameObject playerGameObject)
        {
            Player player = playerGameObject.GetComponent<Player>();
            playersInServer.Remove(player);
            (currentState as RaceSessionState)?.RemovePlayer(player);
        }
    }

    /// <summary>
    /// Race Session State which defines extra interface for handling Race Session Data.
    /// </summary>
    public abstract class RaceSessionState : NetworkedState
    {
        #region Race Session Data
        [SyncVar] protected RaceSessionData raceSessionData = new RaceSessionData(0);
        public double CurrentRaceLength => raceSessionData.CurrentRaceLength;

        /// <summary>
        /// Data passed around between Race and Intermission states (Intermission needs the most recent race data to be displayed).
        /// </summary>
        public readonly struct RaceSessionData
        {
            // Client and Server Properties
            public readonly double raceStartTime;
            public readonly double finishedRaceLength;

            public double CurrentRaceLength => NetworkTime.time - raceStartTime;

            // Server Only Properties
            public List<Player> PlayersInRace { get; }
            public List<Player> FinishedPlayers { get; }
            public IReadOnlyCollection<Player> DeadPlayers => PlayersInRace.Where(p => p.IsDead).ToArray();
            public IEnumerable<Player> PlayersInRaceOrdered
            {
                get
                {
                    return PlayersInRace
                        .OrderBy(player => player.PositionInfo.finishTime)
                        .ThenByDescending(player => player.PositionInfo.Checkpoints.Count)
                        .ThenBy(player =>
                        {
                            Vector3? currCarPosition = player.Car?.transform.position;
                            GameObject[] checkpointsInRace = TrackGeneratorCommon.Singleton.CheckpointsInRace;
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

            public RaceSessionData(double raceStartTime, double finishedRaceLength = 0)
            {
                this.raceStartTime = raceStartTime;
                this.finishedRaceLength = finishedRaceLength;
                this.PlayersInRace = new List<Player>();
                this.FinishedPlayers = new List<Player>();
            }

            public RaceSessionData(double raceStartTime, double finishedRaceLength, List<Player> playersInRace, List<Player> finishedPlayers)
            {
                this.raceStartTime = raceStartTime;
                this.finishedRaceLength = finishedRaceLength;
                this.PlayersInRace = playersInRace;
                this.FinishedPlayers = finishedPlayers;
            }
        }

        [Server]
        public void RemovePlayer(Player player)
        {
            raceSessionData.PlayersInRace.Remove(player);
            raceSessionData.FinishedPlayers.Remove(player);
        }

        #endregion

        #region Leaderboard Synchronisation

        public readonly struct PlayerLeaderboardItemDTO
        {
            public readonly int position;
            public readonly string playerName;
            public readonly string timeString;

            public PlayerLeaderboardItemDTO(int position, string playerName, string timeString = null)
            {
                this.position = position;
                this.playerName = playerName;
                this.timeString = timeString;
            }
        }
        class SyncListPlayerLeaderboardItemDTO : SyncList<PlayerLeaderboardItemDTO> { }
        readonly SyncListPlayerLeaderboardItemDTO leaderboardItems = new SyncListPlayerLeaderboardItemDTO();
        public IReadOnlyCollection<PlayerLeaderboardItemDTO> LeaderboardItems => leaderboardItems;

        [Server]
        protected void UpdateAndSyncLeaderboard()
        {
            leaderboardItems.Clear();
            
            int position = 1;
            foreach (Player player in raceSessionData.PlayersInRaceOrdered)
            {
                if (player.IsDead || player.PositionInfo.IsFinished)
                {
                    leaderboardItems.Add(new PlayerLeaderboardItemDTO(position, player.PlayerName, player.PositionInfo.TimeString));
                }
                else
                {
                    leaderboardItems.Add(new PlayerLeaderboardItemDTO(position, player.PlayerName));
                }

                position++;
            }
        }

        #endregion
    }
}