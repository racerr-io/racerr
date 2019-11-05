using Doozy.Engine.UI;
using Racerr.StateMachine.Server;
using Racerr.UX.Camera;
using TMPro;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    /// <summary>
    /// A state which shows an intermission screen to the user, indicating that they will soon start a race!
    /// </summary>
    public class ClientIntermissionState : LocalState
    {
        [SerializeField] ServerIntermissionState serverIntermissionState;
        [SerializeField] UIView intermissionView;
        [SerializeField] Transform origin;

        // TODO: These items should be extracted to their own script, setting text fields is not the responsibility of this class.
        [SerializeField] TextMeshProUGUI intermissionTimerTMP;
        [SerializeField] TextMeshProUGUI raceTimerTMP;
        [SerializeField] TextMeshProUGUI leaderboardTMP;

        /// <summary>
        /// Upon entering the client intermission state, we will show them the intermission screen which has
        /// race timer and leaderboard info from the previous race.
        /// </summary>
        /// <param name="optionalData">Should be null.</param>
        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();

            // Race Timer. TODO: Extract Race Timer to its own script
            raceTimerTMP.text = serverIntermissionState.FinishedRaceLength.ToRaceTimeFormat();

            // Leaderboard. TODO: Extract Leaderboard to its own script
            string leaderboardText = string.Empty;
            foreach (RaceSessionState.PlayerLeaderboardItemDTO leaderboardItem in serverIntermissionState.LeaderboardItems)
            {
                leaderboardText += $"{leaderboardItem.position}. {leaderboardItem.playerName}";

                if (leaderboardItem.timeString != null)
                {
                    leaderboardText += $" ({leaderboardItem.timeString})";
                }

                leaderboardText += "\n";
            }
            leaderboardTMP.text = leaderboardText;
        }

        /// <summary>
        /// Upon transition to the next view, hide ourselves.
        /// </summary>
        public override void Exit()
        {
            intermissionView.Hide();
        }

        /// <summary>
        /// Called every physics tick to update the intermission timer. Once we see that the Race has started,
        /// transition the client UI to Race mode.
        /// </summary>
        protected override void FixedUpdate()
        {
            // Race Timer. TODO: Extract Race Timer to its own script
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