using Mirror;
using System.Linq;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// When there is no one on the server, the server will be on this state.
    /// </summary>
    public class ServerIdleState : NetworkedState
    {
        [Server]
        public override void Enter(object optionalData = null)
        {
            ServerManager.singleton.DisconnectAIPlayers(ServerStateMachine.Singleton.PlayersInServer.Count);
        }

        /// <summary>
        /// Called every game tick.
        /// Checks whether or not to transition to intermission state, based on if the server has any connected players.
        /// </summary>
        [Server]
        void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.ReadyPlayers.Any(player => !player.IsAI))
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
