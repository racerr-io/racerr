using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// A camera to automatically follow a target.
    /// </summary>
    [ExecuteInEditMode]
    public class TargetObserverCamera : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 3; // How fast the rig will move to keep up with target's position
        [SerializeField] Transform target;    // The target object to follow

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// Update camera on physics tick.
        /// </summary>
        void FixedUpdate()
        {
            FollowTarget();
        }

        /// <summary>
        /// Automatically follow the target strategy.
        /// </summary>
        void FollowTarget()
        {
            // If no target, then we quit early as there is nothing to do (and to protect null references)
            if (target != null)
            {
                // Camera position moves towards target position
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
            }
        }
    }
}
