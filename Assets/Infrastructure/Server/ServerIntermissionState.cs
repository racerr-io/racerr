using Mirror;
using Racerr.Track;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// A state which manages the countdown of the race and displays information of the finished race.
    /// </summary>
    public class ServerIntermissionState : RaceSessionState
    {
        [SerializeField] int intermissionTimerSecondsEditor = 10;
        [SerializeField] int intermissionTimerSeconds = 10;
        [SerializeField] int intermissionTimerSecondsSinglePlayer = 20;

        int intermissionSecondsTotal;
        [SyncVar] int intermissionSecondsRemaining;
        public int IntermissionSecondsRemaining => intermissionSecondsRemaining;
        public double FinishedRaceLength => raceSessionData.finishedRaceLength;

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

            UpdateLeaderboard(); // To ensure the leaderboard has valid info from the new race session data.

#if UNITY_EDITOR
            intermissionSecondsTotal = intermissionTimerSecondsEditor;
#else
            intermissionSecondsTotal = ServerStateMachine.Singleton.ReadyPlayers.Count > 1 ? intermissionTimerSeconds : intermissionTimerSecondsSinglePlayer;
#endif
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
                    TrackGeneratorCommon.Singleton.DestroyIfRequired();
                    TrackGeneratorCommon.Singleton.GenerateIfRequired(ServerStateMachine.Singleton.ReadyPlayers);
                }
            }

            // Intermission Timer fully finished - now we transition to states based on whether or not there are players.
            if (ServerStateMachine.Singleton.PlayersInServer.Any())
            {
                // Only transition to race if track is generated
                while (!TrackGeneratorCommon.Singleton.IsTrackGenerated) yield return null;
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