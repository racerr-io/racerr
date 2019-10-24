using Mirror;
using Racerr.Track;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public class ServerIntermissionState : RaceSessionState
    {
        [SerializeField] int intermissionTimerSecondsEditor;
        [SerializeField] int intermissionTimerSeconds;
        [SerializeField] int intermissionTimerSecondsSinglePlayer;

        int intermissionSecondsTotal;
        [SyncVar] int intermissionSecondsRemaining;

        /// <summary>
        /// Transition function called on entering the intermission state.
        /// Initialises the race session data of the race that just finished, OR null if transitioned from idle state.
        /// Initialises the duration of the intermission based on whether the game is in the Unity Editor or not.
        /// Immediately begins the intermission timer countdown.
        /// </summary>
        /// <param name="raceSessionData">Data of the race that just finished, OR null if transitioned from idle state.</param>
        public override void Enter(object raceSessionData)
        {
            Debug.Log("ENTERING INTERMISSION");
            this.raceSessionData = raceSessionData as RaceSessionData;
#if UNITY_EDITOR
            intermissionSecondsTotal = intermissionTimerSecondsEditor;
#else
            intermissionSecondsTotal = ServerStateMachine.Singleton.ReadyPlayers.Count > 1 ? 
            ServerStateMachine.Singleton.intermissionTimerSeconds : 
            ServerStateMachine.Singleton.intermissionTimerSecondsSinglePlayer;
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
                    TrackGeneratorCommon.Singleton.GenerateIfRequired();
                }
            }

            Debug.Log("PRE PLAYERS IN SERVER CHECK");
            // Intermission Timer fully finished - now we transition to states based on whether or not there are players.
            if (ServerStateMachine.Singleton.PlayersInServer.Any())
            {
                Debug.Log("PRE IS TRACK GEN CHECK");
                // Only transition to race if track is generated
                while (!TrackGeneratorCommon.Singleton.IsTrackGenerated) yield return null;
                Debug.Log("TRANSITION TO RACE");
                TransitionToRace();
            }
            else
            {
                TransitionToIdle();
            }
        }

        void LateUpdate()
        {
            // Leave this here for the tick in the editor
        }

        void TransitionToRace()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.Race);
        }

        void TransitionToIdle()
        {
            ServerStateMachine.Singleton.ChangeState(StateEnum.ServerIdle);
        }
    }
}