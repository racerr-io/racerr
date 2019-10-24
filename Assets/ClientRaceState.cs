using Doozy.Engine.UI;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientRaceState : State
    {
        [SerializeField] UIView raceView;

        public override void Enter(object optionalData = null)
        {
            raceView.Show();
        }

        public override void Exit()
        {
            raceView.Hide();
        }

        protected override void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (Player.LocalPlayer.IsDead)
            {
                TransitionToSpectate();
            }
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }

        void TransitionToSpectate()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.ClientSpectate);
        }
    }
}