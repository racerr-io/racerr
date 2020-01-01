using Doozy.Engine.UI;
using Racerr.Infrastructure.Server;
using Racerr.UX.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are spectating the race. They will spectate the race
    /// either when they join the server for the first time or they are dead.
    /// </summary>
    public class ClientSpectateState : LocalState
    {
        [SerializeField] ServerRaceState serverRaceState;
        [SerializeField] UIView raceView;

        [SerializeField] RaceTimerUIComponent raceTimerUIComponent;
        [SerializeField] CountdownTimerUIComponent countdownTimerUIComponent;
        [SerializeField] LeaderboardUIComponent leaderboardUIComponent;
        [SerializeField] MinimapUIComponent minimapUIComponent;
        [SerializeField] SpectatedPlayerNameUIComponent spectatedPlayerNameUIComponent;

        IEnumerable<Player> playersInRace = null;
        Player spectatedPlayer;

        /// <summary>
        /// Upon entering the spectate state on the client, show the race UI and find all the players we can spectate.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            raceView.Show();
            playersInRace = FindObjectsOfType<Player>().Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
        }

        /// <summary>
        /// Called every physics tick. Updates UI components and who we are spectating, then checks if we should transition to a new client state.
        /// </summary>
        protected override void FixedUpdate()
        {
            SetSpectateTargetIfRequired();
            UpdateUIComponents();
            CheckToTransition();
        }

        /// <summary>
        /// Find a player in the race that we can spectate. From the players in the race, we choose the first player to spectate, ensuring
        /// they haven't finished, died or left the server. The current spectated player's name will be displayed.
        /// </summary>
        void SetSpectateTargetIfRequired()
        {
            if (spectatedPlayer == null || spectatedPlayer.IsDead)
            {
                // playersInRace could be empty, as there is a small window of time where everyone has died/finished but the Server State Machine
                // has not transitioned to intermission yet.
                playersInRace = playersInRace.Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
                spectatedPlayer = playersInRace.FirstOrDefault(); 
                ClientStateMachine.Singleton.SetPlayerCameraTarget(spectatedPlayer?.CarManager.transform);
                minimapUIComponent.SetMinimapCameraTarget(ClientStateMachine.Singleton.LocalPlayer.CarManager.transform);
            }
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
            spectatedPlayerNameUIComponent.UpdateSpectatedPlayerName(spectatedPlayer?.CarManager.name);
        }

        /// <summary>
        /// If the server has changed to intermission, it means we can join the next race! Hence, transition the client to
        /// intermission state.
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
    }
}