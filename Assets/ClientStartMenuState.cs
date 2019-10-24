using System.Collections;
using Doozy.Engine.UI;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientStartMenuState : State
    {
        [SerializeField] UIView startMenuView;

        public override void Enter(object optionalData = null)
        {
            startMenuView.Show();
        }

        public override void Exit()
        {
            startMenuView.Hide();
        }

        public void OnStartRaceButtonClick()
        {
            Player.LocalPlayer.IsReady = true;
            StartCoroutine(WaitUntilServerNotIdleThenTransition());
        }

        IEnumerator WaitUntilServerNotIdleThenTransition()
        {
            while (ServerStateMachine.Singleton.StateType == StateEnum.ServerIdle) yield return null;

            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
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