using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    /// <summary>
    /// A state for the AI racer when they are currently enjoying the race. This includes racer and police.
    /// </summary>
    [RequireComponent(typeof(AIStateMachine))]
    public class AIRaceState : LocalState
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
        /// Transition to the next AI state. 
        /// </summary>
        void CheckToTransition()
        {
            Player player = AIStateMachine.OwnPlayer;

            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (player.PosInfo.IsFinished || player.IsDeadCompletely)
            {
                TransitionToSpectate();
            }
            else if (player.IsDeadAsRacer && player.Health == 0)
            {
                player.CmdSpawnPoliceCarOnFinishingGrid();
            }
        }

        void TransitionToIntermission()
        {
            AIStateMachine.ChangeState(StateEnum.Intermission);
        }

        void TransitionToSpectate()
        {
            AIStateMachine.ChangeState(StateEnum.ClientSpectate);
        }
    }
}