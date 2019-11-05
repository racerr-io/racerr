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
        [SerializeField] float rollSpeed = 0.2f;// How fast the rig will roll (around Z axis) to match target's roll.
        [SerializeField] bool followVelocity = false;// Whether the rig will rotate in the direction of the target's velocity.
        [SerializeField] bool followTilt = true; // Whether the rig will tilt (around X axis) with the target.
        [SerializeField] float spinTurnLimit = 90;// The threshold beyond which the camera stops following the target's rotation. (used in situations where a car spins out, for example)
        [SerializeField] float targetVelocityLowerLimit = 4f;// the minimum velocity above which the camera turns towards the object's velocity. Below this we use the object's forward direction.
        [SerializeField] float smoothTurnTime = 0.2f; // the smoothing for the camera's rotation

        float turnSpeedVelocityChange; // The change in the turn speed velocity
        float LastFlatAngle { get; set; } // The relative angle of the target and the rig from the previous frame.
        float CurrentTurnAmount { get; set; } // How much to turn the camera
        Vector3 RollUp { get; set; } = Vector3.up; // The roll of the camera around the z axis ( generally this will always just be up )

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

        /// <summary>
        /// Automatically follow the target strategy.
        /// </summary>
        /// <param name="deltaTime">Time.DeltaTime</param>
        protected void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || target == null)
            {
                return;
            }

            // initialise some vars, we'll be modifying these in a moment
            var targetForward = target.forward;
            var targetUp = target.up;

            if (followVelocity && Application.isPlaying)
            {
                // in follow velocity mode, the camera's rotation is aligned towards the object's velocity direction
                // but only if the object is traveling faster than a given threshold.

                if (TargetRigidbody.velocity.magnitude > targetVelocityLowerLimit)
                {
                    // velocity is high enough, so we'll use the target's velocty
                    targetForward = TargetRigidbody.velocity.normalized;
                    targetUp = Vector3.up;
                }
                else
                {
                    targetUp = Vector3.up;
                }
                CurrentTurnAmount = Mathf.SmoothDamp(CurrentTurnAmount, 1, ref turnSpeedVelocityChange, smoothTurnTime);
            }
            else
            {
                // we're in 'follow rotation' mode, where the camera rig's rotation follows the object's rotation.

                // This section allows the camera to stop following the target's rotation when the target is spinning too fast.
                // eg when a car has been knocked into a spin. The camera will resume following the rotation
                // of the target when the target's angular velocity slows below the threshold.
                var currentFlatAngle = Mathf.Atan2(targetForward.x, targetForward.z)*Mathf.Rad2Deg;
                if (spinTurnLimit > 0)
                {
                    var targetSpinSpeed = Mathf.Abs(Mathf.DeltaAngle(LastFlatAngle, currentFlatAngle))/deltaTime;
                    var desiredTurnAmount = Mathf.InverseLerp(spinTurnLimit, spinTurnLimit*0.75f, targetSpinSpeed);
                    var turnReactSpeed = (CurrentTurnAmount > desiredTurnAmount ? .1f : 1f);
                    if (Application.isPlaying)
                    {
                        CurrentTurnAmount = Mathf.SmoothDamp(CurrentTurnAmount, desiredTurnAmount,
                                                             ref turnSpeedVelocityChange, turnReactSpeed);
                    }
                    else
                    {
                        // for editor mode, smoothdamp won't work because it uses deltaTime internally
                        CurrentTurnAmount = desiredTurnAmount;
                    }
                }
                else
                {
                    CurrentTurnAmount = 1;
                }
                LastFlatAngle = currentFlatAngle;
            }

            // camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, target.position, deltaTime*moveSpeed);

            // camera's rotation is split into two parts, which can have independend speed settings:
            // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
            if (!followTilt)
            {
                targetForward.y = 0;
                if (targetForward.sqrMagnitude < float.Epsilon)
                {
                    targetForward = transform.forward;
                }
            }

            /*
            var rollRotation = Quaternion.LookRotation(targetForward, RollUp);

            // and aligning with the target object's up direction (i.e. its 'roll')
            RollUp = rollSpeed > 0 ? Vector3.Slerp(RollUp, targetUp, rollSpeed*deltaTime) : Vector3.up;
            transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, turnSpeedVelocityChange * CurrentTurnAmount*deltaTime);
            */
        }
    }
}
