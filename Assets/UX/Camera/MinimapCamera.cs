using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// A camera to automatically follow a target.
    /// </summary>
    [ExecuteInEditMode]
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] Transform target;    // The target object to follow

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// Update camera after every physics tick.
        /// </summary>
        void LateUpdate()
        {
            FollowTarget();
        }

        /// <summary>
        /// Follow the camera's current transform target, by applying a smooth lerp transition.
        /// Y position is fixed.
        /// </summary>
        void FollowTarget()
        {
            // If no target, then we quit early as there is nothing to do (and to protect null references)
            if (target != null)
            {
                Vector3 newPosition = target.position;
                newPosition.y = transform.position.y;
                transform.position = newPosition;
            }
        }
    }
}
