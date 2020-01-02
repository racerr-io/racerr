using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
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
        /// Called upon race finish or player death, where we will hide the Race UI and disable the car controller.
        /// </summary>
        /// <remarks>
        /// On player death, the car will still remain on the track as we do not want the car to just disappear.
        /// Instead, we will manually disable the player's car controller so they can't drive.
        /// </remarks>
        public override void Exit()
        {
            ClientStateMachine.Singleton.LocalPlayer.CarManager?.SetIsActive(false);
            raceView.Hide();
        }

        /// <summary>
        /// Called every physics tick. Updates UI components, then checks if we should transition to a new client state.
        /// </summary>
        protected override void FixedUpdate()
        {
            UpdateUIComponents();
            CheckToTransition();
        }

        /// <summary>
        /// Update all the UI components in the client race view, which shows information about the player's car and how they 
        /// are performing in the race.
        /// </summary>
        void UpdateUIComponents()
        {
            raceTimerUIComponent.UpdateRaceTimer(serverRaceState.CurrentRaceDuration);
            countdownTimerUIComponent.UpdateCountdownTimer(serverRaceState.RemainingRaceTime);
            leaderboardUIComponent.UpdateLeaderboard(serverRaceState.LeaderboardItems);

            CarManager carManager = ClientStateMachine.Singleton.LocalPlayer.CarManager;
            if (carManager != null)
            {
                speedUIComponent.UpdateSpeed(carManager.SpeedKPH);
            }
        }

        /// <summary>
        /// Transition the next client state. If the race is ended, we move to intermission. However, if the race is still going but we
        /// have died or finished the race, we move to spectating.
        /// </summary>
        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (ClientStateMachine.Singleton.LocalPlayer.IsDead || ClientStateMachine.Singleton.LocalPlayer.PosInfo.IsFinished)
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