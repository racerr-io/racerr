using Doozy.Engine.UI;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientStartMenuState : LocalState
    {
        const string defaultPlayerName = "Player";
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
            string playerName = startMenuView.gameObject.GetComponentsInChildren<TMP_InputField>().Single(t => t.name == "Player Name Text").text;
            Player.LocalPlayer.PlayerName = string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName;
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