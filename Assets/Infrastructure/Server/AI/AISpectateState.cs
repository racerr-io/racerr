using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    /// <summary>
    /// A state for the AI when they are spectating the race. They don't really "spectate" it, 
    /// it just a state for when they are fully dead in the race, similar to the human player.
    /// </summary>
    [RequireComponent(typeof(AIStateMachine))]
    public class AISpectateState : LocalState
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
        /// Transition to intermission once the server has transitioned to intermission.
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