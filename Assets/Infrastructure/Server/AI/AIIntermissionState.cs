using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    /// <summary>
    /// The state the AI racer is in when the server is in intermission state.
    /// </summary>
    [RequireComponent(typeof(AIStateMachine))]
    public class AIIntermissionState : LocalState
    {
        AIStateMachine AIStateMachine;

        /// <summary>
        /// Cache the associated AI State Machine.
        /// </summary>
        void Awake()
        {
            AIStateMachine = GetComponent<AIStateMachine>();
        }

        /// <summary>
        /// Called every physics tick to check if we should transition to the next state.
        /// </summary>
        void FixedUpdate()
        {
            CheckToTransition();
        }


        /// <summary>
        /// Transition the next AI state. Once we see that the race has started, transition the 
        /// AI racer to race mode.
        /// </summary>
        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
            {
                TransitionToRace();
            }
        }

        void TransitionToRace()
        {
            AIStateMachine.ChangeState(StateEnum.Race);
        }
    }
}