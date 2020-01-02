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
        [SerializeField] UIView spectateView;

        [SerializeField] RaceTimerUIComponent raceTimerUIComponent;
        [SerializeField] CountdownTimerUIComponent countdownTimerUIComponent;
        [SerializeField] LeaderboardUIComponent leaderboardUIComponent;
        [SerializeField] MinimapUIComponent minimapUIComponent;
        [SerializeField] SpectateInfoUIComponent spectateInfoUIComponent;

        IEnumerable<Player> playersInRace;
        IEnumerable<Player> unspectatedPlayersInRace;
        Player spectatedPlayer;

        /// <summary>
        /// Upon entering the spectate state on the client, show the race UI, spectated player name UI, 
        /// hide the speed UI find all the players we can spectate.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            spectateView.Show();
            playersInRace = FindObjectsOfType<Player>().Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
            unspectatedPlayersInRace = playersInRace;
        }

        /// <summary>
        /// Called upon race finish, where we will hide the spectate UI.
        /// </summary>
        public override void Exit()
        {
            spectateView.Hide();
        }

        /// <summary>
        /// Called every frame tick. Updates who we are spectating.
        /// </summary>
        private void Update()
        {
            SetSpectatedPlayerIfRequired();
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
        /// If we are not spectating or our current spectated player is dead, we choose a player in the race
        /// to spectate, ensuring that they haven't finished, died or left the server. If the spectating player 
        /// decides to change the spectated player (through pressing spacebar), we further ensure spectated
        /// players are not repeated until all players still in the race have been spectated.
        /// </summary>
        void SetSpectatedPlayerIfRequired()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                unspectatedPlayersInRace = unspectatedPlayersInRace.Where(player => player != spectatedPlayer && player != null && !player.IsDead && !player.PosInfo.IsFinished);
                if (!unspectatedPlayersInRace.Any())
                {
                    unspectatedPlayersInRace = playersInRace;
                }
                SetSpectatedPlayer(unspectatedPlayersInRace);
            }

            if (spectatedPlayer == null || spectatedPlayer.IsDead || spectatedPlayer.PosInfo.IsFinished)
            {
                playersInRace = playersInRace.Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
                SetSpectatedPlayer(playersInRace);
            }
        }

        /// <summary>
        /// Find and spectate the first player that we can spectate.
        /// </summary>
        /// <remarks>
        /// spectatablePlayers could be empty, as there is a small window of time where everyone has died/finished 
        /// but the Server State Machine has not transitioned to intermission yet.
        /// </remarks>
        /// <param name="spectatablePlayers"></param>
        void SetSpectatedPlayer(IEnumerable<Player> spectatablePlayers)
        {
            spectatedPlayer = spectatablePlayers.FirstOrDefault();
            ClientStateMachine.Singleton.SetPlayerCameraTarget(spectatedPlayer?.CarManager.transform);
            minimapUIComponent.SetMinimapCameraTarget(spectatedPlayer?.CarManager.transform);
        }

        /// <summary>
        /// Update all the UI components in the client race view, which shows information about the spectated player's car 
        /// and how they are performing in the race.
        /// </summary>
        void UpdateUIComponents()
        {
            raceTimerUIComponent.UpdateRaceTimer(serverRaceState.CurrentRaceDuration);
            countdownTimerUIComponent.UpdateCountdownTimer(serverRaceState.RemainingRaceTime);
            leaderboardUIComponent.UpdateLeaderboard(serverRaceState.LeaderboardItems);
            spectateInfoUIComponent.UpdateSpectateInfo(spectatedPlayer?.PlayerName, playersInRace.Count());
            spectateInfoUIComponent.UpdateSpaceUI();
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