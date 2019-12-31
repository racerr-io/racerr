using Racerr.Utility;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Race Timer, which shows
    /// how much time is remaining in the race.
    /// </summary>
    public class RaceTimerUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI raceTimerTMP;

        /// <summary>
        /// Convert race duration into race time format (M:SS.FFF) and displays it to the user.
        /// </summary>
        /// <param name="currentRaceDuration">Race duration in seconds</param>
        public void UpdateRaceTimer(double currentRaceDuration)
        {
            raceTimerTMP.text = currentRaceDuration.ToRaceTimeFormat();
        }
    }
}