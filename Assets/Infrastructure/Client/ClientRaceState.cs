using Doozy.Engine.UI;
using Racerr.Infrastructure.Server;
using Racerr.UX.UI;
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

        [SerializeField] RaceTimerUIComponent raceTimerUIComponent;
        [SerializeField] CountdownTimerUIComponent countdownTimerUIComponent;
        [SerializeField] SpeedUIComponent speedUIComponent;
        [SerializeField] LeaderboardUIComponent leaderboardUIComponent;
        [SerializeField] MinimapUIComponent minimapUIComponent;

        /// <summary>
        /// Called upon entering the race state on the client, where we show the Race UI.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            raceView.Show();
            ClientStateMachine.Singleton.SetPlayerCameraTarget(ClientStateMachine.Singleton.LocalPlayer.CarManager.transform);
            minimapUIComponent.SetMinimapCameraTarget(ClientStateMachine.Singleton.LocalPlayer.CarManager.transform);
        }

        /// <summary>
        /// Called upon race finish, where we will hide the Race UI.
        /// </summary>
        public override void Exit()
        {
            minimapUIComponent.SetMinimapCameraTarget(null);
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
                raceTimerUIComponent.UpdateRaceTimer(serverRaceState.CurrentRaceDuration);
                countdownTimerUIComponent.UpdateCountdownTimer(serverRaceState.RemainingRaceTime);
                speedUIComponent.UpdateSpeed(ClientStateMachine.Singleton.LocalPlayer.CarManager.SpeedKPH);
                leaderboardUIComponent.UpdateLeaderboard(serverRaceState.LeaderboardItems);
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