using System;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public sealed class ClientStateMachine : StateMachine
    {
        public static ClientStateMachine Singleton;

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one Client State Machine is in the scene.
        /// </summary>
        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Debug.LogError("You can only have one Client State Machine in the scene. The extra one has been destroyed.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Entrypoint into Client State Machine. Called after unity initialises all scripts.
        /// Defaults to the Client Start Menu state, which is the first view the user sees when launching the game.
        /// </summary>
        void Start()
        {
            ChangeState(StateEnum.ClientStartMenu);
        }

        protected override void ChangeStateCore(StateEnum stateType)
        {
            switch (stateType)
            {
                case StateEnum.Intermission: CurrentState = GetComponent<ClientIntermissionState>(); break;
                case StateEnum.Race: CurrentState = GetComponent<ClientRaceState>(); break;
                case StateEnum.ClientSpectate: CurrentState = GetComponent<ClientSpectateState>(); break;
                case StateEnum.ClientStartMenu: CurrentState = GetComponent<ClientStartMenuState>(); break;
                default: throw new InvalidOperationException("Invalid Client ChangeState attempt: " + stateType.ToString());
            }
        }
    }
}