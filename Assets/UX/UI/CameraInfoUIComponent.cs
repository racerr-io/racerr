using Racerr.UX.Camera;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the controlling the primary camera,
    /// which shows information on the current camera state
    /// and how to switch between them.
    /// </summary>
    public class CameraInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI cameraInfoTMP;

        /// <summary>
        /// Given the current state of the primary camera,
        /// update the caption so the user knows what camera
        /// type is currently being used.
        /// </summary>
        /// <param name="cameraType">The current type the primary camera is set to.</param>
        public void UpdateCameraInfo(PrimaryCamera.CameraType cameraType)
        {
            switch (cameraType)
            {
                case PrimaryCamera.CameraType.Overhead: cameraInfoTMP.text = "Overhead"; break;
                case PrimaryCamera.CameraType.ThirdPerson: cameraInfoTMP.text = "Third person"; break;
                case PrimaryCamera.CameraType.KillCam: cameraInfoTMP.text = "Kill cam"; break;
            }
        }
    }
}