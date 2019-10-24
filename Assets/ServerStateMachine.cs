using Mirror;
using Racerr.MultiplayerService;
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
        public StateEnum StateType => stateType;
        NetworkedState currentState;

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
            if (isServer)
            {
                ChangeState(StateEnum.ServerIdle);
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
                this.stateType = stateType;

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
}