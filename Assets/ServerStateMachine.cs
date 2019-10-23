using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public enum ServerStateEnum
    {
        Race,
        Intermission,
        Idle
    }

    public abstract class State : NetworkBehaviour
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
    }

    /// <summary>
    /// Race Session State which defines extra interface for handling Race Session Data.
    /// </summary>
    public abstract class RaceSessionState : State
    {
        /// <summary>
        /// Data passed around between Race and Intermission states (Intermission needs the most recent race data to be displayed).
        /// </summary>
        public sealed class RaceSessionData
        {
            public List<Player> PlayersInRace { get; } = new List<Player>();
            public List<Player> FinishedPlayers { get; } = new List<Player>();
            public IReadOnlyCollection<Player> DeadPlayers => PlayersInRace.Where(p => p.IsDead).ToArray();
            public IEnumerable<Player> PlayersInRaceOrdered
            {
                get
                {
                    return PlayersInRace
                        .OrderBy(player => player.PositionInfo.FinishingTime)
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
        }

        protected RaceSessionData raceSessionData;
        public void RemovePlayer(Player player)
        {
            raceSessionData.PlayersInRace.Remove(player);
            raceSessionData.FinishedPlayers.Remove(player);
        }
    }

    public sealed class ServerStateMachine : NetworkBehaviour
    {
        public static ServerStateMachine Singleton;

        [SyncVar(hook = nameof(OnChangeState))] ServerStateEnum stateType;
        State currentState;

        List<Player> playersInServer = new List<Player>();
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
            ChangeState(ServerStateEnum.Idle);
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

        /// <summary>
        /// Changes the state of the Server State Machine.
        /// Intended to be PROTECTED - only the Server States should be able to call this from their encapsulated transition methods.
        /// Changes the internal state of the Server State Machine based on the given state type Enum.
        /// </summary>
        /// <param name="stateType">The new state type to be changed to.</param>
        /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
        public void ChangeState(ServerStateEnum stateType, object optionalData = null)
        {
            if (currentState != null)
            {
                // Only time when the current state will be null is when the server starts.
                currentState.Exit();
                currentState.enabled = false;
            }

            this.stateType = stateType;

            switch (stateType)
            {
                case ServerStateEnum.Race: currentState = GetComponent<RaceState>(); break;
                case ServerStateEnum.Intermission: currentState = GetComponent<IntermissionState>(); break;
                case ServerStateEnum.Idle: currentState = GetComponent<IdleState>(); break;
                default: Debug.LogError("Invalid State"); break;
            }

            currentState.enabled = true;
            currentState.Enter(optionalData);
        }

        /// <summary>
        /// Hook for stateType SyncVar.
        /// Executed on clients when server changes the state.
        /// </summary>
        /// <param name="stateType">Default hook parameter (the updated variable)</param>
        public void OnChangeState(ServerStateEnum stateType)
        {
            ChangeState(stateType);
        }
    }
}