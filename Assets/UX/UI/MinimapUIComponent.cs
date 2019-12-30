using UnityEngine;

namespace Racerr.UX.UI
{
    public class MinimapUIComponent : MonoBehaviour
    {
        [SerializeField] MinimapCamera minimapCamera;

        /// <summary>
        /// Move the minimap camera's target to point to the target transform.
        /// Should only be called by client states.
        /// </summary>
        /// <param name="targetTransform">New target transform</param>
        public void SetMinimapCameraTarget(Transform targetTransform)
        {
            minimapCamera.Target = targetTransform;
        }
    }
}
