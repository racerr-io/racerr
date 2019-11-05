using System.Linq;
using Racerr.MultiplayerService;
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
        [SerializeField] bool autoTargetPlayer = true;  // Whether the rig should automatically target the player.

        public Transform Target => target;

        /// <summary>
        /// if auto targeting is used, find the object tagged "Player"
        /// any class inheriting from this should call base.Start() to perform this action!
        /// </summary>
        void Start()
        {
            if (autoTargetPlayer)
            {
                FindAndTargetPlayer();
            }
        }

        /// <summary>
        /// Update camera on physics tick.
        /// </summary>
        void FixedUpdate()
        {
            if (autoTargetPlayer && (target == null || !target.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }

            FollowTarget(Time.deltaTime);
        }

        /// <summary>
        /// Auto target an object tagged player, if no target has been assigned
        /// </summary>
        public void FindAndTargetPlayer()
        {
            Player alivePlayer = FindObjectsOfType<Player>().Where(player => player.IsReady && !player.IsDead && player.Car != null).FirstOrDefault();
            SetTarget(alivePlayer?.Car.gameObject.transform);
        }

        /// <summary>
        /// Set a new target
        /// </summary>
        /// <param name="newTransform">Transform you want to target</param>
        public void SetTarget(Transform newTransform)
        {
            target = newTransform;
        }

        /// <summary>
        /// Automatically follow the target strategy.
        /// </summary>
        /// <param name="deltaTime">Time.DeltaTime</param>
        void FollowTarget(float deltaTime)
        {
            // If no target, then we quit early as there is nothing to do
            if (target == null)
            {
                return;
            }

            // Camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, target.position, deltaTime * moveSpeed);
        }
    }
}
