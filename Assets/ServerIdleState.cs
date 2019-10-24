using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public class ServerIdleState : State
    {
        /// <summary>
        /// Called every game tick.
        /// Checks whether or not to transition to intermission state, based on if the server has any connected players.
        /// </summary>
        void LateUpdate()
        {
            if (ServerStateMachine.Singleton.PlayersInServer.Any(p => p.IsReady))
            {
                TransitionToIntermission();
            }
        }

        void TransitionToIntermission()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }
    }
}
