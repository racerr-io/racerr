﻿using Doozy.Engine.UI;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.UX.Camera;
using Racerr.UX.UI;
using Racerr.World.Track;
using System;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state which shows an intermission screen to the user, indicating that they will soon start a race!
    /// </summary>
    public class ClientIntermissionState : LocalState
    {
        [SerializeField] ServerIntermissionState serverIntermissionState;
        [SerializeField] UIView intermissionView;
        [SerializeField] RaceTimerUIComponent raceTimerUIComponent;
        [SerializeField] LeaderboardUIComponent leaderboardUIComponent;
        [SerializeField] IntermissionTimerUIComponent intermissionTimerUIComponent;
        [SerializeField] CameraInfoUIComponent cameraInfoUIComponent;

        /// <summary>
        /// Upon entering the client intermission state, we will show them the intermission screen which has
        /// race timer and leaderboard info from the previous race. We will also attach a callback function
        /// which moves the camera to the latest generated track as soon as it is generated by the server, 
        /// so the client can see the cool track generation algorithm.
        /// </summary>
        /// <param name="optionalData">Should be null.</param>
        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();
            TrackGenerator.Singleton.GeneratedTrackPieces.Callback += SetCameraTargetOnTrackPieceGenerated;
            TrackGenerator.Singleton.TrackGenerated += SetCameraTargetOnEntireTrackGenerated;
            UpdateUIComponentsWithPreviousRaceInformation();
        }

        /// <summary>
        /// Upon transition to the next view, hide ourselves. Also detach the callback function which moves
        /// the camera to the latest generated track to prevent the camera from having its target reassigned.
        /// </summary>
        public override void Exit()
        {
            TrackGenerator.Singleton.GeneratedTrackPieces.Callback -= SetCameraTargetOnTrackPieceGenerated;
            TrackGenerator.Singleton.TrackGenerated -= SetCameraTargetOnEntireTrackGenerated;
            intermissionView.Hide();
        }

        /// <summary>
        /// Delegate function that should be attached to the callback of Track Generator's Generated Track Pieces Sync List.
        /// It is called automatically when the Sync List updates on the server.
        /// The purpose of this is to move the camera to the latest generated track on the client.
        /// </summary>
        /// <param name="op">Operation Type</param>
        /// <param name="itemIndex">The index of the newly added track (unused)</param>
        /// <param name="oldItem">The track itself that was removed (unused in this case)</param>
        /// <param name="newItem">The track itself that was added</param>
        void SetCameraTargetOnTrackPieceGenerated(Mirror.SyncList<GameObject>.Operation op, int itemIndex, GameObject oldItem, GameObject newItem)
        {
            if (op == Mirror.SyncList<GameObject>.Operation.OP_ADD)
            {
                ClientStateMachine.Singleton.PrimaryCamera.SetTarget(newItem.transform, PrimaryCamera.CameraType.Overhead);
            }
        }

        /// <summary>
        /// Delegate function that should be attached to the callback of Track Generator's Track Generated event.
        /// It is called when a new track is fully generated.
        /// The purpose of this is to move the camera to the player's car in third person view to prepare for the
        /// race to start.
        /// </summary>
        /// <param name="sender">The track generator (unused).</param>
        /// <param name="e">Event args (empty and unused).</param>
        void SetCameraTargetOnEntireTrackGenerated(object sender, EventArgs e)
        {
            // Race condition sometimes occurs where the last track piece event is sent after the whole track
            // is generated, so detach the track piece generated function.
            TrackGenerator.Singleton.GeneratedTrackPieces.Callback -= SetCameraTargetOnTrackPieceGenerated;

            CarManager carManager = ClientStateMachine.Singleton.LocalPlayer.CarManager;
            if (carManager != null)
            {
                ClientStateMachine.Singleton.PrimaryCamera.SetTarget(carManager.transform, PrimaryCamera.CameraType.ThirdPerson);
            }
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
        /// Update all the UI components with previous race information, with information from the previous race so players
        /// can be informed of how they and opponents performed in the previous race, while they wait.
        /// </summary>
        void UpdateUIComponentsWithPreviousRaceInformation()
        {
            raceTimerUIComponent.UpdateRaceTimer(serverIntermissionState.FinishedRaceDuration);
            leaderboardUIComponent.UpdateLeaderboard(serverIntermissionState.LeaderboardItems);
        }

        /// <summary>
        /// Update all the UI components with information regarding the intermission.
        /// </summary>
        void UpdateUIComponents()
        {
            cameraInfoUIComponent.UpdateCameraInfo(ClientStateMachine.Singleton.PrimaryCamera.CamType);
            intermissionTimerUIComponent.UpdateIntermissionTimer(serverIntermissionState.IntermissionSecondsRemaining);
        }

        /// <summary>
        /// Transition the next client state. Once we see that the race has started, transition the 
        /// client to race mode.
        /// </summary>
        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
            {
                if (ClientStateMachine.Singleton.LocalPlayer.CarManager != null)
                {
                    TransitionToRace();
                }
                else
                {
                    TransitionToSpectate();
                }
            }
        }

        void TransitionToRace()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Race);
        }

        void TransitionToSpectate()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.ClientSpectate);
        }
    }
}
