using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    /// <summary>
    /// Manages the currently running race. Keeps track of all the Players in the race and
    /// ensures transition to Idle / Intermission once all players have left or finished the race.
    /// </summary>
    public class ServerRaceState : RaceSessionState
    {
        // Flag to keep track of whether we are racing or not to guard the transitions in FixedUpdate from being called.
        bool isCurrentlyRacing;

        /// <summary>
        /// Initialises brand new race session data independent of previous race sessions.
        /// Then starts the race, assuming track has already been generated during intermission state.
        /// </summary>
        [Server]
        public override void Enter(object optionalData = null)
        {
            isCurrentlyRacing = false;
            raceSessionData = new RaceSessionData(NetworkTime.time);
            StartRace();
        }

        /// <summary>
        /// When exiting the race, destroy any players which remain on the track. 
        /// Note that players remain on the track after the race from dying (their uncontrollable corpse remains).
        /// </summary>
        [Server]
        public override void Exit()
        {
            foreach (Player player in raceSessionData.PlayersInRace)
            {
                if (player.Car != null)
                {
                    player.DestroyPlayersCar();
                }
            }
        }

        /// <summary>
        /// Procedure to actually setup and start the race.
        /// </summary>
        [Server]
        void StartRace()
        {
            Vector3 currPosition = new Vector3(0, 1, 10);
            raceSessionData.PlayersInRace.AddRange(ServerStateMachine.Singleton.ReadyPlayers);

            foreach (Player player in raceSessionData.PlayersInRace)
            {
                player.CreateCarForPlayer(currPosition);
                player.PositionInfo = new PlayerPositionInfo(raceSessionData.raceStartTime);
                currPosition += new Vector3(0, 0, 10);
            }

            isCurrentlyRacing = true;
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
            player.PositionInfo = new PlayerPositionInfo(player.PositionInfo.startTime, NetworkTime.time);
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
            player.PositionInfo.Checkpoints.Add(checkpoint);

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
            if (isCurrentlyRacing)
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

