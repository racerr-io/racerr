using System.Linq;
using Doozy.Engine.UI;
using Mirror;
using Racerr.StateMachine.Server;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientIntermissionState : State
    {
        [SerializeField] ServerIntermissionState serverIntermissionState;
        [SerializeField] UIView intermissionView;
        TextMeshProUGUI intermissionTimerTMP;

        public void Start()
        {
            intermissionTimerTMP = intermissionView.gameObject.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "Text (TMP) - Intermission Timer");
        }

        [Client]
        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();
        }

        [Client]
        public override void Exit()
        {
            intermissionView.Hide();
        }

        [Client]
        protected override void FixedUpdate()
        {
            intermissionTimerTMP.text = serverIntermissionState.IntermissionSecondsRemaining.ToString();
            if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
            {
                TransitionToRace();
            }
        }

        [Client]
        void TransitionToRace()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Race);
        }
    }
}