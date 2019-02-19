using Mirror;
using Racerr.MultiplayerService;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.HUD
{
    /// <summary>
    /// Timer for beginning and ending the race.
    /// </summary>
    public class RaceTimer : NetworkBehaviour
    {
        const string TimerLabel = "Timer Label";
        const string WaitingForPlayersLabel = "Waiting For Players Label";
        const string GameStatusLabel = "Game Status Label";

        [SyncVar(hook = "OnChangeSecondsRemaining")] int secondsRemaining;

        Text secondsRemainingText;
        GameObject waitingForPlayersLabelGO;
        GameObject timerLabelGO;
        GameObject gameStatusLabelGO;

        /// <summary>
        /// Initialise race timer with the labels.
        /// </summary>
        void Start()
        {
            waitingForPlayersLabelGO = transform.Find(WaitingForPlayersLabel).gameObject;
            timerLabelGO = transform.Find(TimerLabel).gameObject;
            gameStatusLabelGO = transform.Find(GameStatusLabel).gameObject;
            secondsRemainingText = timerLabelGO.GetComponent<Text>();
        }

        /// <summary>
        /// Check every frame on client whether we should display spectating.
        /// </summary>
        void Update()
        {
            if (isClient)
            {
                UpdateSpectatingLabel();
            }
        }

        /// <summary>
        /// Start the race timer and subsequently the race.
        /// </summary>
        /// <param name="seconds">Seconds to count down.</param>
        [Server]
        public void StartTimer(int seconds)
        {
            secondsRemaining = seconds;

            if (isServer)
            {
                StartCoroutine(CountdownTimer());
            }
        }

        /// <summary>
        /// Coroutine for counting down the timer.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        [Server]
        IEnumerator CountdownTimer()
        {
            while (secondsRemaining > 0)
            {
                yield return new WaitForSeconds(1);
                secondsRemaining--;
            }

            RacerrRaceSessionManager.Singleton.StartRace();
        }

        /// <summary>
        /// Hook for secondsRemaining SyncVar. When this value changes,
        /// determine the state of the timer.
        /// </summary>
        /// <param name="secondsRemaining">The new value</param>
        [Client]
        void OnChangeSecondsRemaining(int secondsRemaining)
        {
            this.secondsRemaining = secondsRemaining;

            if (secondsRemaining > 0 && Player.LocalPlayer.IsReady)
            {
                ShowTimer();
                secondsRemainingText.text = secondsRemaining.ToString();
                waitingForPlayersLabelGO.SetActive(secondsRemaining > 5);
            }
            else
            {
                HideTimer();
            }
        }

        /// <summary>
        /// Check whether the Spectating label should be shown.
        /// </summary>
        [Client]
        void UpdateSpectatingLabel()
        {
            if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing && Player.LocalPlayer.IsReady && Player.LocalPlayer.Car == null)
            {
                gameStatusLabelGO.SetActive(true);
            }
            else
            {
                gameStatusLabelGO.SetActive(false);
            }
        }

        /// <summary>
        /// Show the timer UI 
        /// (note the waiting for players is determined in OnChangeSecondsRemaining() hook)
        /// </summary>
        [Client]
        void ShowTimer()
        {
            timerLabelGO.SetActive(true);
        }

        /// <summary>
        /// Hide the timer UI.
        /// </summary>
        [Client]
        void HideTimer()
        {
            waitingForPlayersLabelGO.SetActive(false);
            timerLabelGO.SetActive(false);
        }
    }
}