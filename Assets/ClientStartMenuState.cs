using System.Collections;
using System.Linq;
using Doozy.Engine.UI;
using Mirror;
using Racerr.MultiplayerService;
using Racerr.StateMachine.Server;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientStartMenuState : State
    {
        const string defaultPlayerName = "Player";
        [SerializeField] UIView startMenuView;

        [Client]
        public override void Enter(object optionalData = null)
        {
            startMenuView.Show();
        }

        [Client]
        public override void Exit()
        {
            startMenuView.Hide();
        }

        [Client]
        public void OnStartRaceButtonClick()
        {
            Player.LocalPlayer.IsReady = true;
            string playerName = startMenuView.gameObject.GetComponentsInChildren<TMP_InputField>().Single(t => t.name == "Player Name Text").text;
            Player.LocalPlayer.PlayerName = string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName;
            StartCoroutine(WaitUntilServerNotIdleThenTransition());
        }

        [Client]
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