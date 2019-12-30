using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    public class SpeedUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI speedTMP;

        public void UpdateSpeed(double speedKPH)
        {
            speedTMP.text = Convert.ToInt32(speedKPH).ToString() + " KPH";
        }
    }
}