using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.Utility;
using System;
using TMPro;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are currently enjoying the race. Intended to show them race information,
    /// and information about themselves.
    /// </summary>
    public class ClientRaceState : LocalState
    {
        [SerializeField] ServerRaceState serverRaceState;
        [SerializeField] UIView raceView;
        [SerializeField] int countdownTimeThreshold = 10;

        // TODO: These items should be extracted to their own script, setting text fields is not the responsibility of this class.
        [SerializeField] TextMeshProUGUI raceTimerTMP;
        [SerializeField] TextMeshProUGUI countdownTimerTMP;
        [SerializeField] TextMeshProUGUI speedTMP;
        [SerializeField] TextMeshProUGUI leaderboardTMP;

        /// <summary>
        /// Called upon entering the race state on the client, where we show the Race UI.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            raceView.Show();
            ClientStateMachine.Singleton.SetPlayerCameraTarget(ClientStateMachine.Singleton.LocalPlayer.Car.transform);
            ClientStateMachine.Singleton.SetMinimapCameraTarget(ClientStateMachine.Singleton.LocalPlayer.Car.transform);
        }

        /// <summary>
        /// Called upon race finish, where we will hide the Race UI.
        /// </summary>
        public override void Exit()
        {
            ClientStateMachine.Singleton.SetMinimapCameraTarget(null);
            ClientStateMachine.Singleton.SetPlayerCameraTarget(null);
            raceView.Hide();
        }

        /// <summary>
        /// Called every physics tick.
        /// If the client discovers the race has ended, we move the client to Intermission State.
        /// If the client discovers the race is still going, but we are dead or finished the race, we move the client to Spectate State.
        /// Otherwise, it means the race is still going and we are still racing, so we will update the UI elements accordingly.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (ClientStateMachine.Singleton.LocalPlayer.IsDead || ClientStateMachine.Singleton.LocalPlayer.PosInfo.IsFinished)
            {
                TransitionToSpectate();
            }
            else
            {
                // Race Timer. TODO: Extract Race Timer to its own script
                raceTimerTMP.text = serverRaceState.CurrentRaceDuration.ToRaceTimeFormat();

                // Countdown Timer. TODO: Extract Countdown Timer to its own script
                if (serverRaceState.RemainingRaceTime > countdownTimeThreshold)
                {
                    countdownTimerTMP.gameObject.SetActive(false);
                }
                else
                {
                    countdownTimerTMP.gameObject.SetActive(true);
                    countdownTimerTMP.text = Math.Ceiling(serverRaceState.RemainingRaceTime).ToString();
                }

                // Speed. TODO: Extract Speed to its own script
                speedTMP.text = Convert.ToInt32(ClientStateMachine.Singleton.LocalPlayer.Car.SpeedKPH).ToString() + " KPH";

                // Leaderboard. TODO: Extract Leaderboard to its own script
                string leaderboardText = string.Empty;
                foreach (RaceSessionState.PlayerLeaderboardItemDTO leaderboardItem in serverRaceState.LeaderboardItems)
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