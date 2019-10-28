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

        /// <summary>
        /// Show the start menu view to the user.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            startMenuView.Show();
        }

        /// <summary>
        /// Hide the start menu view upon transition.
        /// </summary>
        public override void Exit()
        {
            startMenuView.Hide();
        }

        /// <summary>
        /// When the user clicks 'RACE!' in the UI, this function is called
        /// by Reflection from Doozy UI Button Listener on click.
        /// </summary>
        public void OnStartRaceButtonClick()
        {
            Player.LocalPlayer.IsReady = true;
            string playerName = startMenuView.gameObject.GetComponentsInChildren<TMP_InputField>().Single(t => t.name == "Player Name Text").text;
            Player.LocalPlayer.PlayerName = string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName;
            StartCoroutine(WaitUntilServerNotIdleThenTransition());
        }

        /// <summary>
        /// Upon clicking the RACE button, wait until the server knows that there is more than zero players
        /// on the server (indicated by a transition away from Idle) before we transition.
        /// </summary>
        /// <returns>IEnumerator for Coroutine</returns>
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