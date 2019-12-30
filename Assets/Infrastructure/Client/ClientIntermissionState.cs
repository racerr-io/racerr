using Doozy.Engine.UI;
using Racerr.Infrastructure.Server;
using Racerr.UX.UI;
using Racerr.World.Track;
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

        /// <summary>
        /// Upon entering the client intermission state, we will show them the intermission screen which has
        /// race timer and leaderboard info from the previous race.
        /// </summary>
        /// <param name="optionalData">Should be null.</param>
        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();
            TrackGeneratorCommon.Singleton.GeneratedTrackPieces.Callback += SetCameraTargetOnTrackGenerated;
            raceTimerUIComponent.UpdateRaceTimer(serverIntermissionState.FinishedRaceDuration);
            leaderboardUIComponent.UpdateLeaderboard(serverIntermissionState.LeaderboardItems);
        }

        /// <summary>
        /// Upon transition to the next view, hide ourselves.
        /// </summary>
        public override void Exit()
        {
            TrackGeneratorCommon.Singleton.GeneratedTrackPieces.Callback -= SetCameraTargetOnTrackGenerated;
            intermissionView.Hide();
        }

        /// <summary>
        /// Delegate function that should be attached to the callback of Track Generator's Generated Track Pieces Sync List.
        /// It is called automatically when the Sync List updates on the server.
        /// The purpose of this is to move the camera to the latest generated track on the client.
        /// </summary>
        /// <param name="op">Operation Type</param>
        /// <param name="itemIndex">The index of the newly added track (unused)</param>
        /// <param name="item">The track itself that was added</param>
        void SetCameraTargetOnTrackGenerated(Mirror.SyncList<GameObject>.Operation op, int itemIndex, GameObject item)
        {
            if (op == Mirror.SyncList<GameObject>.Operation.OP_ADD)
            {
                ClientStateMachine.Singleton.SetPlayerCameraTarget(item.transform);
            }
        }

        /// <summary>
        /// Called every physics tick to update the intermission timer. Once we see that the Race has started,
        /// transition the client UI to Race mode.
        /// </summary>
        protected override void FixedUpdate()
        {
            intermissionTimerUIComponent.UpdateIntermissionTimer(serverIntermissionState.IntermissionSecondsRemaining);

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
