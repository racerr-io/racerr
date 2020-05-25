using UnityEngine;

namespace Racerr.Infrastructure.Server.AI
{
    [RequireComponent(typeof(AIStateMachine))]
    public class AIRaceState : LocalState
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