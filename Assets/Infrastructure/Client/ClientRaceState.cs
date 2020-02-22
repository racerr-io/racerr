using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.UX.Camera;
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
        [SerializeField] CameraInfoUIComponent cameraInfoUIComponent;
        [SerializeField] RearViewMirrorUIComponent rearViewMirrorUIComponent;

        /// <summary>
        /// Called upon entering the race state on the client, where we show the Race UI.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            raceView.Show();
            ClientStateMachine.Singleton.PrimaryCamera.SetTarget(ClientStateMachine.Singleton.LocalPlayer.CarManager.transform, PrimaryCamera.CameraType.ThirdPerson);
            minimapUIComponent.SetMinimapCameraTarget(ClientStateMachine.Singleton.LocalPlayer.CarManager.transform);
        }

        /// <summary>
        /// Called upon race finish or player death, where we will hide the Race UI.
        /// </summary>
        public override void Exit()
        {
            raceView.Hide();
        }

        /// <summary>
        /// Called every frame tick. Updates who we are spectating and the UI components. We need to call put these things
        /// in here instead of FixedUpdate() so updates to the UI are not choppy and inputs are accurate.
        /// </summary>
        void Update()
        {
            UpdateUIComponents();
        }

        /// <summary>
        /// Called every physics tick to check if we should transition to the next state.
        /// </summary>
        void FixedUpdate()
        {
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
            cameraInfoUIComponent.UpdateCameraInfo(ClientStateMachine.Singleton.PrimaryCamera.CamType);
            rearViewMirrorUIComponent.UpdateRearViewMirror(ClientStateMachine.Singleton.PrimaryCamera);

            CarManager carManager = ClientStateMachine.Singleton.LocalPlayer.CarManager;
            if (carManager != null)
            {
                speedUIComponent.UpdateSpeed(carManager.SpeedKPH);
            }
        }

        /// <summary>
        /// Transition the next client state. If the race is ended, we move to intermission. However, if we crossed the finish line,
        /// move to spectate, but if we died, show the death view.
        /// </summary>
        void CheckToTransition()
        {
            Player player = ClientStateMachine.Singleton.LocalPlayer;

            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (player.PosInfo.IsFinished)
            {
                TransitionToSpectate();
            }
            else if (player.Health == 0)
            {
                TransitionToDeath();
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

        void TransitionToDeath()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.ClientDeath);
        }
    }
}