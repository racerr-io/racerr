using Racerr.Infrastructure.Server;
using System.Linq;
using System.Collections.Generic;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are spectating the race. They will spectate the race
    /// either when they join the server for the first time or they are dead.
    /// </summary>
    public class ClientSpectateState : LocalState
    {
        IEnumerable<Player> playersInServer;

        /// <summary>
        /// Upon entering the spectate state on the client, check if there are any players in the race that we can spectate.
        /// Store this as a variable so we can check if we are already spectating.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            playersInServer = FindObjectsOfType<Player>().Where(player => !player.IsDead && !player.PosInfo.IsFinished);
        }

        /// <summary>
        /// Called every physics tick to monitor the server state. If the server has changed to intermission,
        /// it means we can join the next race! Hence, update our UI to the Intermission State.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }

            ClientStateMachine.Singleton.SetPlayerCameraTarget(playersInServer.First().CarManager.transform);
            ClientStateMachine.Singleton.SetMinimapCameraTarget(playersInServer.First().CarManager.transform);
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }
    }
}