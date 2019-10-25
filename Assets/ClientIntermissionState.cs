using Doozy.Engine.UI;
using Racerr.StateMachine.Server;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientIntermissionState : LocalState
    {
        [SerializeField] ServerIntermissionState serverIntermissionState;
        [SerializeField] UIView intermissionView;
        [SerializeField] TextMeshProUGUI intermissionTimerTMP;
        [SerializeField] TextMeshProUGUI raceTimerTMP;

        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();

            if (serverIntermissionState.PreviousRaceLength != null)
            {
                raceTimerTMP.text = serverIntermissionState.PreviousRaceLength.Value.ToRaceTimeFormat();
            }
            else
            {
                raceTimerTMP.text = 0d.ToRaceTimeFormat();
            }
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