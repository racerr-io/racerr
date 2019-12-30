using Racerr.Utility;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    public class RaceTimerUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI raceTimerTMP;

        public void UpdateRaceTimer(double currentRaceDuration)
        {
            raceTimerTMP.text = currentRaceDuration.ToRaceTimeFormat();
        }
    }
}