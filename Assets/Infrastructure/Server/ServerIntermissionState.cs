using Mirror;
using Racerr.World.Track;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// A state which manages the countdown of the race and displays information of the finished race.
    /// </summary>
    public class ServerIntermissionState : RaceSessionState
    {
        [SerializeField] int intermissionTimerSecondsEditor = 1;
        [SerializeField] int intermissionTimerSeconds = 10;
        [SerializeField] int intermissionTimerSecondsSinglePlayer = 20;
        [SerializeField] int minPlayersOnServer = 10;

        int intermissionSecondsTotal;
        [SyncVar] int intermissionSecondsRemaining;
        public int IntermissionSecondsRemaining => intermissionSecondsRemaining;
        public double FinishedRaceDuration => raceSessionData.finishedRaceDuration;

        /// <summary>
        /// Transition function called on entering the intermission state.
        /// Initialises the race session data of the race that just finished, OR empty race session data if transitioned from idle state.
        /// Initialises the duration of the intermission based on whether the game is in the Unity Editor or not.
        /// Immediately begins the intermission timer countdown.
        /// </summary>
        /// <param name="raceSessionData">Data of the race that just finished, OR null if transitioned from idle state.</param>
        [Server]
        public override void Enter(object raceSessionData)
        {
            if (raceSessionData != null)
            {
                this.raceSessionData = (RaceSessionData)raceSessionData;
            }
            else
            {
                this.raceSessionData = new RaceSessionData(0);
            }

            // To ensure the leaderboard has valid info from the new race session data.
            UpdateLeaderboard();

            if (Application.isEditor)
            {
                intermissionSecondsTotal = intermissionTimerSecondsEditor;
            }    
            else
            {
                intermissionSecondsTotal = ServerStateMachine.Singleton.ReadyPlayers.Count(player => !player.IsAI) > 1 
                    ? intermissionTimerSeconds 
                    : intermissionTimerSecondsSinglePlayer;
            }

            StartCoroutine(IntermissionTimerAndTrackGeneration());
        }

        /// <summary>
        /// Coroutine for counting down the intermission timer.
        /// When timer reaches half time, the previous track is destroyed and a new one
        /// When timer reaches 0, forces a state change depending on whether or not there are players.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        [Server]
        IEnumerator IntermissionTimerAndTrackGeneration()
        {
            intermissionSecondsRemaining = intermissionSecondsTotal;
            while (intermissionSecondsRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                intermissionSecondsRemaining--;

                // Destroy previous track and generate new track for next race when halfway in intermission
                if (intermissionSecondsRemaining == intermissionSecondsTotal / 2)
                {
                    TrackGenerator.Singleton.DestroyIfRequired();

                    // Spawn/despawn required AI players
                    int numToSpawn = minPlayersOnServer - ServerStateMachine.Singleton.ReadyPlayers.Count;
                    if (numToSpawn > 0)
                    {
                        ServerManager.singleton.ConnectAIPlayers(numToSpawn);
                    }
                    else
                    {
                        ServerManager.singleton.DisconnectAIPlayers(-numToSpawn);
                    }

                    List<Player> shuffledReadyPlayers = ServerStateMachine.Singleton.ReadyPlayers.OrderBy(_ => Guid.NewGuid()).ToList();
                    TrackGenerator.Singleton.GenerateIfRequired(shuffledReadyPlayers);
                }
            }

            // Intermission Timer fully finished - now we transition to states based on whether or not there are players.
            if (ServerStateMachine.Singleton.ReadyPlayers.Any(player => !player.IsAI))
            {
                // Only transition to race if track is generated
                while (!TrackGenerator.Singleton.IsTrackGenerated) yield return null;
                TransitionToRace();
            }
            else
            {
                TransitionToIdle();
            }
        }

        [Server]
        void TransitionToRace()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.Race);
        }

        [Server]
        void TransitionToIdle()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.ServerIdle);
        }
    }
}