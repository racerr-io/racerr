using Doozy.Engine.UI;
using Racerr.StateMachine.Server;
using Racerr.UX.Camera;
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
        [SerializeField] TextMeshProUGUI leaderboardTMP;
        [SerializeField] Transform origin;

        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();

            raceTimerTMP.text = serverIntermissionState.PreviousRaceLength.ToRaceTimeFormat();

            string leaderboardText = string.Empty;
            foreach (RaceSessionState.PlayerPositionDTO playerPositionDTO in serverIntermissionState.playerPositions)
            {
                leaderboardText += $"{playerPositionDTO.position}. {playerPositionDTO.playerName}";

                if (playerPositionDTO.timeString != null)
                {
                    leaderboardText += $" ({playerPositionDTO.timeString})";
                }

                leaderboardText += "\n";
            }

            leaderboardTMP.text = leaderboardText;

            FindObjectOfType<AutoCam>().SetTarget(origin); // dodgy code fix l8r
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