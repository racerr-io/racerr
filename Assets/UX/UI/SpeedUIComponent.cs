using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Speed, which shows how fast the user's
    /// car is travelling as a simple text component.
    /// </summary>
    public class SpeedUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI speedTMP;

        /// <summary>
        /// Given the speed, display it as an integer with the units,
        /// which is currently kilometres per hour.
        /// </summary>
        /// <param name="speedKPH">Speed in kilometres per hour</param>
        public void UpdateSpeed(double speedKPH)
        {
            speedTMP.text = Math.Round(speedKPH).ToString() + " KPH";
        }
    }
}