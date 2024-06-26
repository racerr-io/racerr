﻿using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Intermission Timer, which is a text component shown on the screen
    /// during intermission indicating to the user that we are in 
    /// Intermission state and the timer remaining until the start of the race.
    /// </summary>
    public class IntermissionTimerUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI intermissionTimerTMP;

        /// <summary>
        /// Given the number of seconds left for intermission, update the time shown
        /// on the intermission timer so that the user can ready themselves for race start.
        /// </summary>
        /// <param name="intermissionSecondsRemaining">Time remaining in intermission, in seconds</param>
        public void UpdateIntermissionTimer(int intermissionSecondsRemaining)
        {
            intermissionTimerTMP.text = intermissionSecondsRemaining.ToString();
        }
    }
}

