using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.Utility;
using Racerr.UX.Camera;
using Racerr.UX.UI;
using Racerr.World.Track;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when the player dies (in racer or police mode)
    /// and shows who killed them for a short period of time before transitioning
    /// to the next state.
    /// </summary>
    public class ClientDeathState : LocalState
    {
        [SerializeField] UIView deathView;
        [SerializeField] int duration = 5;
        [SerializeField] CameraInfoUIComponent cameraInfoUIComponent;
        [SerializeField] DeathInfoUIComponent deathInfoUIComponent;

        bool allowTransition = true;

        /// <summary>
        /// Called upon entering the death state on the client, where we show the death UI.
        /// We will focus the camera and death UI on the player who last hit us. If no player
        /// last hit us (e.g. we crashed into many buildings), it will focus on the current player.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            deathView.Show();

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
            allowTransition = true;
        }

        /// <summary>
        /// Upon exiting the death state, hide the death UI.
        /// </summary>
        public override void Exit()
        {
            deathView.Hide();
            allowTransition = false;
        }

        /// <summary>
        /// Called every frame tick. Updates the UI components. We need to call put these things
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
        /// Update all the UI components in the death view, which is just the camera info. Other UI elements
        /// are not in the death view so the user is not distracted.
        /// </summary>
        void UpdateUIComponents()
        {
            cameraInfoUIComponent.UpdateCameraInfo(ClientStateMachine.Singleton.PrimaryCamera.CamType);
        }

        /// <summary>
        /// This should be called in Enter(). After a specified period of time, transition the racer to either
        /// spectate mode (if they died completely) or race mode (if they are respawning as a police car).
        /// Note that it is possible for the server to transition to intermission at any time, so if this occurs
        /// we need to ensure to cancel the async operation. This is done through a boolean.
        /// </summary>
        void TransitionAfterWaitingPeriod()
        {
            this.YieldThenExecuteAsync(new WaitForSeconds(duration), () =>
            {
                if (allowTransition)
                {
                    Player player = ClientStateMachine.Singleton.LocalPlayer;
                    if (player.IsDeadCompletely)
                    {
                        TransitionToSpectate();
                    }
                    else if (player.IsDeadAsRacer)
                    {
                        GameObject finishPosition = SpawnManager.GetPoliceCarOnFinishingGridPosition(player);
                        player.CmdCreatePoliceCarForPlayer(finishPosition.transform.position, finishPosition.transform.rotation);
                        SpawnManager.policeCarsOnFinishingGrid.Add(player);

                        // Wait for the server to spawn the car before we transition to race.
                        this.WaitForConditionThenExecuteAsync(() => player.Health > 0, TransitionToRace);
                    }
                }
            });
        }

        /// <summary>
        /// Transition to intermission at any time, if the server changes to intermission state
        /// mid way. Cancels the waiting period above.
        /// </summary>
        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
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