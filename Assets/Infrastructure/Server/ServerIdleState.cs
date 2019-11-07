using Mirror;
using System.Linq;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// When there is no one on the server, the server will be on this state.
    /// </summary>
    public class ServerIdleState : NetworkedState
    {
        /// <summary>
        /// Called every game tick.
        /// Checks whether or not to transition to intermission state, based on if the server has any connected players.
        /// </summary>
        [Server]
        protected override void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.PlayersInServer.Any(p => p.IsReady))
            {
                TransitionToIntermission();
            }
        }

        [Server]
        void TransitionToIntermission()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }
    }
}
