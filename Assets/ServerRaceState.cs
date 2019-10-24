using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public class ServerRaceState : RaceSessionState
    {
        bool isCurrentlyRacing;

        /// <summary>
        /// Initialises brand new race session data independant of previous race sessions.
        /// Then starts generating the track, which will then start the race.
        /// </summary>
        [Server]
        public override void Enter(object optionalData = null)
        {
            isCurrentlyRacing = false;
            raceSessionData = new RaceSessionData();
            StartRace();
        }

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
        /// Called only after track is generated.
        /// </summary>
        [Server]
        void StartRace()
        {
            Vector3 currPosition = new Vector3(0, 1, 10);
            raceSessionData.PlayersInRace.AddRange(ServerStateMachine.Singleton.ReadyPlayers);

            foreach (Player player in raceSessionData.PlayersInRace)
            {
                player.CreateCarForPlayer(currPosition);
                player.PositionInfo = new PlayerPositionInfo();
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
            player.PositionInfo.FinishingTime = NetworkTime.time;
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
            }
        }

        [Server]
        public void TransitionToIntermission()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.Intermission, raceSessionData);
        }

        [Server]
        public void TransitionToIdle()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.ServerIdle);
        }
    }
}

