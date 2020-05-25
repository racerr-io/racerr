using System;
using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    /// <summary>
    /// State Machine used to control the current state of an AI racer.
    /// Each AI racer has its own state machine, similar to how each client has its own Client State Machine.
    /// The AI state machines are available only on the server for each AI player.
    /// </summary>
    public sealed class AIStateMachine : MonoBehaviour, IStateMachine
    {
        public StateEnum StateType { get; private set; }
        public Player OwnPlayer { get; private set; }

        LocalState currentState;

        /// <summary>
        /// Grab the Player associated with this AI State Machine.
        /// </summary>
        void Awake()
        {
            OwnPlayer = GetComponentInParent<Player>();
        }

        /// <summary>
        /// Entrypoint into AI State Machine. Called after unity initialises all scripts.
        /// Defaults to the Intermission state, since that is the only time AI players can spawn.
        /// </summary>
        void Start()
        {
            ChangeState(StateEnum.Intermission);
        }

        /// <summary>
        /// Changes the state of the AI State Machine.
        /// Intended to be PROTECTED - only the AI States should be able to call this from their encapsulated transition methods.
        /// Changes the internal state of the AI State Machine based on the given state type Enum.
        /// </summary>
        /// <param name="stateType">The new state type to be changed to.</param>
        /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
        public void ChangeState(StateEnum stateType, object optionalData = null)
        {
            if (currentState != null)
            {
                // Only time when the current state will be null is when state machine spawns.
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
