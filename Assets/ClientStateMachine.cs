using Mirror;
using System;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public sealed class ClientStateMachine : MonoBehaviour, IStateMachine
    {
        public static ClientStateMachine Singleton;

        public StateEnum StateType { get; private set; }
        LocalState currentState;

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
            if (!NetworkManager.isHeadless)
            {
                ChangeState(StateEnum.ClientStartMenu);
            }
            else
            {
                Destroy(gameObject);
            }
        }

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
                    case StateEnum.Intermission: currentState = GetComponent<ClientIntermissionState>(); break;
                    case StateEnum.Race: currentState = GetComponent<ClientRaceState>(); break;
                    case StateEnum.ClientSpectate: currentState = GetComponent<ClientSpectateState>(); break;
                    case StateEnum.ClientStartMenu: currentState = GetComponent<ClientStartMenuState>(); break;
                    default: throw new InvalidOperationException("Invalid Client ChangeState attempt: " + stateType.ToString());
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
    }
}