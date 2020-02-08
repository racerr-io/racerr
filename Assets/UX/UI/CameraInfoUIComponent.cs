using Racerr.UX.Camera;
using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Speed, which shows how fast the user's
    /// car is travelling as a simple text component.
    /// </summary>
    public class CameraInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI cameraInfoTMP;

        /// <summary>
        /// Given the speed, display it as an integer with the units,
        /// which is currently kilometres per hour.
        /// </summary>
        /// <param name="speedKPH">Speed in kilometres per hour</param>
        public void UpdateCameraInfo(PrimaryCamera.CameraType cameraType)
        {
            if (cameraType == PrimaryCamera.CameraType.Overhead)
            {
                cameraInfoTMP.text = "Overhead";
            }
            else if (cameraType == PrimaryCamera.CameraType.ThirdPerson)
            {
                cameraInfoTMP.text = "Third person";
            }
        }
    }
}