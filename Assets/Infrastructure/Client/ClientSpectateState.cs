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

        IEnumerable<Player> opponentPlayers;
        Queue<Player> opponentPlayersNotSpectated;
        Player currentlySpectatedOpponent;

        /// <summary>
        /// Upon entering the spectate state on the client, show the spectate UI and 
        /// find all opponent players in the race.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            currentlySpectatedOpponent = null;
            opponentPlayers = FindObjectsOfType<Player>().Where(player => IsPlayerConnectedAndRacing(player) && player != ClientStateMachine.Singleton.LocalPlayer);
            opponentPlayersNotSpectated = new Queue<Player>(opponentPlayers);
            spectateView.Show();
        }

        /// <summary>
        /// Called upon race finish, where we will hide the spectate UI.
        /// </summary>
        public override void Exit()
        {
            spectateView.Hide();
        }

        /// <summary>
        /// Called every frame tick. Updates who we are spectating and the UI components. We need to call put these things
        /// in here instead of FixedUpdate() so updates to the UI are not choppy and inputs are accurate.
        /// </summary>
        void Update()
        {
            SetSpectatedPlayerIfRequired();
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
        /// We would like to know which players have not disconnected from the server (the null check)
        /// and are alive and racing in the game (the IsRacing property) so we can have an updated list
        /// of unspectated players.
        /// </summary>
        /// <param name="player">The player object.</param>
        /// <returns>Boolean representing whether they are alive and racing.</returns>
        bool IsPlayerConnectedAndRacing(Player player) => player != null && player.IsRacing;

        /// <summary>
        /// If our current spectated opponent is not connected and racing or the spectating player
        /// decides to change the current spectated opponent (through pressing spacebar), we choose an opponent 
        /// in the race that we have not spectated yet.
        /// </summary>
        /// <remarks>
        /// If we have went through our whole queue of opponent players that are not spectated already but
        /// the race has not finished (i.e. if the player has changed the spectating opponent themselves through
        /// spacebar), then we reinitialise our queue to be the opponent players still in the race.
        /// </remarks>
        void SetSpectatedPlayerIfRequired()
        {
            opponentPlayers = opponentPlayers.Where(IsPlayerConnectedAndRacing);

            if (!IsPlayerConnectedAndRacing(currentlySpectatedOpponent) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!opponentPlayersNotSpectated.Any())
                {
                    opponentPlayersNotSpectated = new Queue<Player>(opponentPlayers);
                }

                Player opponentToSpectate = null;
                while (opponentToSpectate == null && opponentPlayersNotSpectated.Any())
                {
                    opponentToSpectate = opponentPlayersNotSpectated.Dequeue();
                }

                if (opponentToSpectate != null)
                {
                    SetCurrentlySpectatedOpponent(opponentToSpectate);
                }
            }
        }

        /// <summary>
        /// Spectate the given opponent.
        /// </summary>
        /// <param name="opponentToSpectate"></param>
        void SetCurrentlySpectatedOpponent(Player opponentToSpectate)
        {
            currentlySpectatedOpponent = opponentToSpectate;
            ClientStateMachine.Singleton.SetPlayerCameraTarget(opponentToSpectate.CarManager.transform);
            minimapUIComponent.SetMinimapCameraTarget(opponentToSpectate.CarManager.transform);
        }

        /// <summary>
        /// Update all the UI components in the client race view, which shows information about the spectated opponent's car 
        /// and how they are performing in the race.
        /// </summary>
        void UpdateUIComponents()
        {
            raceTimerUIComponent.UpdateRaceTimer(serverRaceState.CurrentRaceDuration);
            countdownTimerUIComponent.UpdateCountdownTimer(serverRaceState.RemainingRaceTime);
            leaderboardUIComponent.UpdateLeaderboard(serverRaceState.LeaderboardItems);
            spectateInfoUIComponent.UpdateSpectateInfo(currentlySpectatedOpponent?.PlayerName, opponentPlayers.Count());
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