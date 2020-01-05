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
            opponentPlayers = FindObjectsOfType<Player>().Where(player => IsSpectatable(player) && player != ClientStateMachine.Singleton.LocalPlayer);
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
            SetSpectatedOpponentIfRequired();
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
        /// Tells us whether a player is spectatable, which is when they
        /// haven't disconnected from the server and they haven't finished or died.
        /// </summary>
        /// <param name="player">The player object.</param>
        /// <returns>Boolean representing whether they are alive and racing.</returns>
        bool IsSpectatable(Player player) => player != null && player.IsRacing;

        /// <summary>
        /// If our current spectated opponent disconnects or the spectating player swaps the current spectated opponent 
        /// (through pressing spacebar), we choose an opponent in the race that we have not spectated yet, by popping them
        /// off an unspectated queue. If this queue is empty, we will reinitialise it with all the opponent players.
        /// This allows the user to cycle through the opponent players.
        /// </summary>
        void SetSpectatedOpponentIfRequired()
        {
            opponentPlayers = opponentPlayers.Where(IsSpectatable);

            if (!IsSpectatable(currentlySpectatedOpponent) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!opponentPlayersNotSpectated.Any())
                {
                    opponentPlayersNotSpectated = new Queue<Player>(opponentPlayers);
                }

                Player opponentToSpectate = null;
                while (!IsSpectatable(opponentToSpectate) && opponentPlayersNotSpectated.Any())
                {
                    opponentToSpectate = opponentPlayersNotSpectated.Dequeue();
                }

                if (IsSpectatable(opponentToSpectate))
                {
                    SetCurrentlySpectatedOpponent(opponentToSpectate);
                }
            }
        }

        /// <summary>
        /// Spectate the given opponent, by changing the main camera and minimap camera to target them.
        /// </summary>
        /// <param name="opponentToSpectate">Opponent Player</param>
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