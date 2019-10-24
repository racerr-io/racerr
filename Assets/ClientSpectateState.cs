using Racerr.StateMachine.Server;

namespace Racerr.StateMachine.Client
{
    public class ClientSpectateState : LocalState
    {
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