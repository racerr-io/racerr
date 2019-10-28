using Racerr.StateMachine.Server;

namespace Racerr.StateMachine.Client
{
    /// <summary>
    /// A state for the client when they are spectating the race. They will spectate the race
    /// either when they join the server for the first time or they are dead.
    /// </summary>
    public class ClientSpectateState : LocalState
    {
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
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }
    }
}