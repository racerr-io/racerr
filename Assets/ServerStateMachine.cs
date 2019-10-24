using Mirror;
using Racerr.MultiplayerService;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public sealed class ServerStateMachine : StateMachine
    {
        public static ServerStateMachine Singleton;
        protected override bool IsStatesActivatable => isServer;

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
            ChangeState(StateEnum.ServerIdle);
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
            (CurrentState as RaceSessionState)?.RemovePlayer(player);
        }

        protected override void ChangeStateCore(StateEnum stateType)
        {
            switch (stateType)
            {
                case StateEnum.Intermission: CurrentState = GetComponent<ServerIntermissionState>(); break;
                case StateEnum.Race: CurrentState = GetComponent<ServerRaceState>(); break;
                case StateEnum.ServerIdle: CurrentState = GetComponent<ServerIdleState>(); break;
                default: throw new InvalidOperationException("Invalid Server ChangeState attempt: " + stateType.ToString());
            }
        }
    }
}