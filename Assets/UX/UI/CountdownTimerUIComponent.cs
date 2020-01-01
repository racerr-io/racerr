using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Countdown Timer, which is a large number which shows up in the middle of screen.
    /// It is intended to be displayed when the race is nearly finished.
    /// </summary>
    public class CountdownTimerUIComponent : MonoBehaviour
    {
        [SerializeField] int countdownTimeThreshold = 10;
        [SerializeField] TextMeshProUGUI countdownTimerTMP;

        /// <summary>
        /// Given the remaining race time, determine if we are near the end of the race and
        /// display the countdown timer as a simple integer to the user. We only wish to show 
        /// the timer if the race is nearly finished.
        /// </summary>
        /// <param name="remainingRaceTime">Remaining race time in seconds</param>
        public void UpdateCountdownTimer(double remainingRaceTime)
        {
            if (remainingRaceTime > countdownTimeThreshold)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                countdownTimerTMP.text = Math.Ceiling(remainingRaceTime).ToString();
            }
        }
    }
}