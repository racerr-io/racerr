using Doozy.Engine.UI;
using Mirror;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientRaceState : State
    {
        [SerializeField] UIView raceView;

        [Client]
        public override void Enter(object optionalData = null)
        {
            raceView.Show();
        }

        [Client]
        public override void Exit()
        {
            raceView.Hide();
        }

        [Client]
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

        [Client]
        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }

        [Client]
        void TransitionToSpectate()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.ClientSpectate);
        }
    }
}