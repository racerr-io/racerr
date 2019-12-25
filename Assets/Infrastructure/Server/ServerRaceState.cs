using Mirror;
using Racerr.Gameplay.Car;
using Racerr.World.Track;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// Manages the currently running race. Keeps track of all the Players in the race and
    /// ensures transition to Idle / Intermission once all players have left or finished the race.
    /// <remarks>
    /// Assumes the track has already been generated before transitioning into this state.
    /// </remarks>
    /// </summary>
    public class ServerRaceState : RaceSessionState
    {
        /// <summary>
        /// Initialises brand new race session data independent of previous race sessions.
        /// </summary>
        [Server]
        public override void Enter(object optionalData = null)
        {
            raceSessionData = new RaceSessionData(NetworkTime.time);
            EnableAllPlayerCarControllers();
        }

        /// <summary>
        /// When exiting the race, destroy any players which remain on the track. 
        /// Note that players remain on the track after the race from dying (their uncontrollable corpse remains).
        /// </summary>
        [Server]
        public override void Exit()
        {
            foreach (Player player in raceSessionData.PlayersInRace.Where(player => player.Car != null))
            {
                player.DestroyPlayersCar();
            }
        }

        /// <summary>
        /// Server side only - call this function once the car finishes the race so that
        /// their car is removed and they are marked as finished.
        /// </summary>
        /// <param name="player">The player that finished.</param>
        [Server]
        public void NotifyPlayerFinished(Player player)
        {
            raceSessionData.FinishedPlayers.Add(player);
            player.PosInfo = new Player.PositionInfo(player.PosInfo.startTime, NetworkTime.time);
            player.DestroyPlayersCar();
        }

        /// <summary>
        /// Server side only - call this function when the car moves through a checkpoint.
        /// Adds checkpoint to a set of checkpoints the player has passed through so that
        /// we can calculate their position.
        /// Additionally, check if the player has actually finished the race.
        /// </summary>
        /// <param name="player">The player that passed through.</param>
        /// <param name="checkpoint">The checkpoint the player hit.</param>
        [Server]
        public void NotifyPlayerPassedThroughCheckpoint(Player player, GameObject checkpoint)
        {
            player.PosInfo.Checkpoints.Add(checkpoint);

            if (checkpoint.name == TrackPieceComponent.FinishLineCheckpoint)
            {
                NotifyPlayerFinished(player);
            }
        }

        /// <summary>
        /// Called every game tick.
        /// Checks whether or not to transition to intermission state, based on if the race is finished or empty.
        /// </summary>
        [Server]
        protected override void FixedUpdate()
        {
            bool isRaceFinished = raceSessionData.FinishedPlayers.Count + raceSessionData.DeadPlayers.Count == raceSessionData.PlayersInRace.Count;
            bool isRaceEmpty = raceSessionData.PlayersInRace.Count == 0;

            if (isRaceEmpty)
            {
                TransitionToIdle();
            }
            else if (isRaceFinished)
            {
                TransitionToIntermission();
            }

            UpdateLeaderboard(); // Ensure clients have a live updated view of the leaderboard always.
        }

        /// <summary>
        /// By default, cars are instantiated in disabled state, meaning any input from the client's controller is ignored
        /// and the car won't move. This is done because we don't want people driving while the track is generating before the race
        /// has started. This function will allow car's to be driven, as we have now entered the Race State.
        /// </summary>
        [Server]
        void EnableAllPlayerCarControllers()
        {
            foreach (Player player in raceSessionData.PlayersInRace.Where(player => player.Car != null))
            {
                player.Car.GetComponent<CarController>().RpcSetCarActiveState(true);
            }
        }

        [Server]
        public void TransitionToIntermission()
        {
            // Copy over the Race Session Data to the Intermission State so the previous race's leaderboard and timer
            // is shown while players wait for the next race.
            RaceSessionData raceSessionDataForIntermission = new RaceSessionData(
                raceSessionData.raceStartTime,
                CurrentRaceLength,
                raceSessionData.PlayersInRace,
                raceSessionData.FinishedPlayers
            );
            ServerStateMachine.Singleton.ChangeState(StateEnum.Intermission, raceSessionDataForIntermission);
        }

        [Server]
        public void TransitionToIdle()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.ServerIdle);
        }
    }
}

