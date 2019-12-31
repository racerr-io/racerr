using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Minimap, which shows a bird's eye view of a target
    /// and their surroundings in the world.
    /// </summary>
    public class MinimapUIComponent : MonoBehaviour
    {
        [SerializeField] MinimapCamera minimapCamera;

        /// <summary>
        /// Move the minimap camera's target to point to the target transform.
        /// </summary>
        /// <param name="targetTransform">New target transform</param>
        public void SetMinimapCameraTarget(Transform targetTransform)
        {
            minimapCamera.Target = targetTransform;
        }
    }
}
