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
        /// Update camera every frame, after all Update()'s have been called, so that the camera's position is
        /// as accurate as possible.
        /// </summary>
        void LateUpdate()
        {
            FollowTarget();
        }

        /// <summary>
        /// Follow the camera's current transform target, by applying a smooth lerp transition.
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
