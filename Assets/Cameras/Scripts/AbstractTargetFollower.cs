using Racerr.Car.Core;
using Racerr.MultiplayerService;
using System.Linq;
using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// Abstract class for camera which follows targets.
    /// </summary>
    public abstract class AbstractTargetFollower : MonoBehaviour
    {
        public enum UpdateType // The available methods of updating are:
        {
            FixedUpdate, // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate, // Update in LateUpdate. (for tracking objects that are moved in Update)
            ManualUpdate, // user must call to update camera
        }

        [SerializeField] protected Transform target;    // The target object to follow
        [SerializeField] bool autoTargetPlayer = true;  // Whether the rig should automatically target the player.
        [SerializeField] UpdateType updateType;         // stores the selected update type

        public Transform Target => target;
        protected Rigidbody TargetRigidbody { get; set; }

        /// <summary>
        /// if auto targeting is used, find the object tagged "Player"
        /// any class inheriting from this should call base.Start() to perform this action!
        /// </summary>
        protected virtual void Start()
        {
            if (autoTargetPlayer)
            {
                FindAndTargetPlayer();
            }
            if (target == null) return;
            TargetRigidbody = target.GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Update camera on physics tick.
        /// </summary>
        void FixedUpdate()
        {
            UpdateCore(UpdateType.FixedUpdate);
        }

        /// <summary>
        /// Called after all update functions called.
        /// </summary>
        void LateUpdate()
        {
            UpdateCore(UpdateType.LateUpdate);
        }

        /// <summary>
        /// Manually update the camera bassed on function call.
        /// </summary>
        public void ManualUpdate()
        {
            UpdateCore(UpdateType.ManualUpdate);
        }

        /// <summary>
        /// We update from here if updatetype is set to Late, or in auto mode,
        /// if the target does not have a rigidbody, or - does have a rigidbody but is set to kinematic.
        /// </summary>
        void UpdateCore(UpdateType updateType)
        {
            if (autoTargetPlayer && (target == null || !target.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }
            if (this.updateType == updateType)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        /// <summary>
        /// Some strategy to operate the camera.
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        protected abstract void FollowTarget(float deltaTime);

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
        public virtual void SetTarget(Transform newTransform)
        {
            target = newTransform;
        }
    }
}
