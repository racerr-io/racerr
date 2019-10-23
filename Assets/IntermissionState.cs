using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine.Server
{
    public class IntermissionState : RaceSessionState
    {
        [SerializeField] int intermissionTimerSecondsEditor;
        [SerializeField] int intermissionTimerSeconds;
        [SerializeField] int intermissionTimerSecondsSinglePlayer;

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
            this.raceSessionData = raceSessionData as RaceSessionData;
#if UNITY_EDITOR
            intermissionSecondsRemaining = intermissionTimerSecondsEditor;
#else
        intermissionSecondsRemaining = ServerStateMachine.Singleton.ReadyPlayers.Count > 1 ? 
            ServerStateMachine.Singleton.intermissionTimerSeconds : 
            ServerStateMachine.Singleton.intermissionTimerSecondsSinglePlayer;
#endif
            StartCoroutine(IntermissionTimer());
        }

        /// <summary>
        /// Coroutine for counting down the intermission timer.
        /// When timer reaches 0, forces a state change depending on whether or not there are players.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        [Server]
        IEnumerator IntermissionTimer()
        {
            while (intermissionSecondsRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                intermissionSecondsRemaining--;
            }

            // Intermission Timer fully finished - now we transition to states based on whether or not there are players.
            if (raceSessionData.PlayersInRace.Any())
            {
                TransitionToRace();
            }
            else
            {
                TransitionToIdle();
            }
        }

        void LateUpdate()
        {
            
        }

        void TransitionToRace()
        {
            ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Race);
        }

        void TransitionToIdle()
        {
            ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Idle);
        }
    }
}