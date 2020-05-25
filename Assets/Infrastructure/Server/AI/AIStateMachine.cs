using System;
using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    /// <summary>
    /// State Machine used to control the current state of the race on the server.
    /// Used by the Server to basically manage everything related to multiplayer.
    /// Intended to be synchronised to clients, so that they can read the StateType
    /// and change the experience they provide to the user accordingly.
    /// Note that this state will disable itself on clients, so Unity script
    /// functions do not run (networking is still active).
    /// </summary>
    public sealed class AIStateMachine : MonoBehaviour, IStateMachine
    {
        public StateEnum StateType { get; private set; }
        public Player OwnPlayer { get; private set; }

        LocalState currentState;

        void Awake()
        {
            OwnPlayer = GetComponentInParent<Player>();
        }

        /// <summary>
        /// Entrypoint into Client State Machine. Called after unity initialises all scripts.
        /// Defaults to the Client Start Menu state, which is the first view the user sees when launching the game.
        /// </summary>
        void Start()
        {
            ChangeState(StateEnum.Intermission);
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
                    case StateEnum.Intermission: currentState = GetComponent<AIIntermissionState>(); break;
                    case StateEnum.Race: currentState = GetComponent<AIRaceState>(); break;
                    case StateEnum.ClientSpectate: currentState = GetComponent<AISpectateState>(); break;
                    default: throw new InvalidOperationException("Invalid AI ChangeState attempt: " + stateType.ToString());
                }
                SentrySdk.AddBreadcrumb($"AI State Machine change state from { StateType } to { stateType }.");
                StateType = stateType;
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError(e);
            }

            currentState.enabled = true;
            currentState.Enter(optionalData);
        }
    }
}
