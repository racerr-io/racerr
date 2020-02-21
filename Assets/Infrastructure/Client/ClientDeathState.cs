using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.Utility;
using Racerr.UX.Camera;
using Racerr.UX.UI;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are currently enjoying the race. Intended to show them race information,
    /// and information about themselves.
    /// </summary>
    public class ClientDeathState : LocalState
    {
        [SerializeField] UIView killCamView;

        [SerializeField] int duration = 5;
        [SerializeField] CameraInfoUIComponent cameraInfoUIComponent;
        [SerializeField] DeathInfoUIComponent deathInfoUIComponent;

        bool cancelWaitingPeriodTransition = false;

        /// <summary>
        /// Called upon entering the race state on the client, where we show the Race UI.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            killCamView.Show();

            CarManager playerCarManager = ClientStateMachine.Singleton.LocalPlayer.CarManager;
            Player killer = playerCarManager.LastHitByPlayer;
            if (killer != null)
            {
                bool showRevengeInstruction = playerCarManager.CarType == CarManager.CarTypeEnum.Racer;
                deathInfoUIComponent.UpdateDeathInfo(killer.PlayerName, showRevengeInstruction);
                ClientStateMachine.Singleton.PrimaryCamera.SetTarget(killer.CarManager.transform, PrimaryCamera.CameraType.Death);
            }
            else
            {
                deathInfoUIComponent.UpdateDeathInfo(null, false);
                ClientStateMachine.Singleton.PrimaryCamera.SetTarget(playerCarManager.transform, PrimaryCamera.CameraType.Death);
            }
            
            TransitionAfterWaitingPeriod();
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
            killCamView.Hide();
            cancelWaitingPeriodTransition = false;
        }

        /// <summary>
        /// Called every frame tick. Updates who we are spectating and the UI components. We need to call put these things
        /// in here instead of FixedUpdate() so updates to the UI are not choppy and inputs are accurate.
        /// </summary>
        void Update()
        {
            UpdateUIComponents();
        }

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
            cameraInfoUIComponent.UpdateCameraInfo(ClientStateMachine.Singleton.PrimaryCamera.CamType);
        }

        /// <summary>
        /// Transition the next client state. If the race is ended, we move to intermission. However, if the race is still going but we
        /// have died or finished the race, we move to spectating.
        /// </summary>
        void TransitionAfterWaitingPeriod()
        {
            UnityEngineHelper.AsyncYieldThenExecute(this, new WaitForSeconds(duration), () =>
            {
                if (!cancelWaitingPeriodTransition)
                {
                    Player player = ClientStateMachine.Singleton.LocalPlayer;
                    if (player.IsDeadCompletely)
                    {
                        TransitionToSpectate();
                    }
                    else if (player.IsDeadAsRacer)
                    {
                        player.CmdCreatePoliceCarForPlayer();
                        TransitionToRace();
                    }
                }
            });
        }

        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                cancelWaitingPeriodTransition = true;
                TransitionToIntermission();
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

        void TransitionToRace()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Race);
        }
    }
}