using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    public class CountdownTimerUIComponent : MonoBehaviour
    {
        [SerializeField] int countdownTimeThreshold = 10;
        [SerializeField] TextMeshProUGUI countdownTimerTMP;

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