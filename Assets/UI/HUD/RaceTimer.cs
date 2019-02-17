using Mirror;
using Racerr.MultiplayerService;
using Racerr.RaceSessionManager;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.HUD
{
    public class RaceTimer : NetworkBehaviour
    {
        const string TimerLabel = "Timer Label";
        const string WaitingForPlayersLabel = "Waiting For Players Label";
        const string GameStatusLabel = "Game Status Label";

        [SyncVar(hook = "OnChangeSecondsRemaining")] int secondsRemaining;

        Text secondsRemainingText;
        GameObject waitingForPlayersLabelGameObject;
        GameObject timerLabelGameObject;
        GameObject gameStatusLabelGameObject;

        void Start()
        {
            waitingForPlayersLabelGameObject = transform.Find(WaitingForPlayersLabel).gameObject;
            timerLabelGameObject = transform.Find(TimerLabel).gameObject;
            gameStatusLabelGameObject = transform.Find(GameStatusLabel).gameObject;
            secondsRemainingText = timerLabelGameObject.GetComponent<Text>();

            if (isClient)
            {
                InvokeRepeating("UpdateSpectatingLabel", 0f, 5f);
            }
        }

        [Server]
        public void StartTimer(int seconds)
        {
            secondsRemaining = seconds;

            if (isServer)
            {
                StartCoroutine(CountdownTimer());
            }
        }

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

        [Client]
        void UpdateSpectatingLabel()
        {
            if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing && Player.LocalPlayer.IsReady && Player.LocalPlayer.Car == null)
            {
                gameStatusLabelGameObject.SetActive(true);
            }
            else
            {
                gameStatusLabelGameObject.SetActive(false);
            }
        }

        void OnChangeSecondsRemaining(int secondsRemaining)
        {
            if (isServerOnly) return;

            this.secondsRemaining = secondsRemaining;

            if (secondsRemaining > 0 && Player.LocalPlayer.IsReady)
            {
                ShowTimer();
                secondsRemainingText.text = secondsRemaining.ToString();
                waitingForPlayersLabelGameObject.SetActive(secondsRemaining > 5);
            }
            else
            {
                HideTimer();
            }
        }

        void ShowTimer()
        {
            timerLabelGameObject.SetActive(true);
        }

        void HideTimer()
        {
            waitingForPlayersLabelGameObject.SetActive(false);
            timerLabelGameObject.SetActive(false);
        }
    }
}