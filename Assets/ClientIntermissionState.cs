using Doozy.Engine.UI;
using Racerr.StateMachine.Server;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientIntermissionState : LocalState
    {
        [SerializeField] ServerIntermissionState serverIntermissionState;
        [SerializeField] UIView intermissionView;
        TextMeshProUGUI intermissionTimerTMP;

        public void Start()
        {
            intermissionTimerTMP = intermissionView.gameObject.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "Text (TMP) - Intermission Timer");
        }

        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();
        }

        public override void Exit()
        {
            intermissionView.Hide();
        }

        protected override void FixedUpdate()
        {
            intermissionTimerTMP.text = serverIntermissionState.IntermissionSecondsRemaining.ToString();
            if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
            {
                TransitionToRace();
            }
        }

        void TransitionToRace()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Race);
        }
    }
}