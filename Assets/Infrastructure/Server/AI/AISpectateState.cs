using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    [RequireComponent(typeof(AIStateMachine))]
    public class AISpectateState : LocalState
    {
        AIStateMachine AIStateMachine;

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
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
        }

        void TransitionToIntermission()
        {
            AIStateMachine.ChangeState(StateEnum.Intermission);
        }
    }
}