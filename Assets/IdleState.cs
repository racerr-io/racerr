using System.Linq;

namespace Racerr.StateMachine.Server
{
    public class IdleState : State
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
            ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Intermission);
        }
    }
}
