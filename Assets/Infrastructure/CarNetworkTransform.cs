using Mirror;
using Racerr.Gameplay.Car;
using UnityEngine;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Custom designed interpolation for car driving.
    /// </summary>
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(Rigidbody))]
    public class CarNetworkTransform : NetworkBehaviour
    {
        [SerializeField] [Range(0, 1)] float interpolationFactor = 0.4f;
        Rigidbody vehicleRigidbody;

        /* These fields are the ones we want updated on every car. */
        [SyncVar(hook = nameof(UpdatePosition))] Vector3 realPosition = Vector3.zero;
        [SyncVar(hook = nameof(UpdateRotation))] Quaternion realRotation;
        [SyncVar(hook = nameof(UpdateVelocity))] Vector3 realVelocity;
        [SyncVar(hook = nameof(UpdateAngularVelocity))] Vector3 realAngularVelocity;

        /// <summary>
        /// Called when car is instantiated. If car is someone else's car remove the wheel colliders
        /// as they intefere with movement.
        /// </summary>
        void Start()
        {
            vehicleRigidbody = GetComponent<Rigidbody>();
        }

        #region SyncVar Hooks
        /* Hooks are called on every client when the server updates a SyncVar. However we don't want to call the hook
           on the client that is in control of the car. */

        /// <summary>
        /// SyncVar hook for position updates.
        /// </summary>
        /// <param name="realPosition">The new position.</param>
        void UpdatePosition(Vector3 realPosition)
        {
            if (!hasAuthority)
            {
                Vector3 predictedPosition = realPosition + Time.deltaTime * realVelocity;
                this.realPosition = transform.position = Vector3.Lerp(transform.position, predictedPosition, interpolationFactor);
            }
        }

        /// <summary>
        /// SyncVar hook for rotation updates.
        /// </summary>
        /// <param name="realRotation">The new rotation.</param>
        void UpdateRotation(Quaternion realRotation)
        {
            if (!hasAuthority)
            {
                this.realRotation = transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, interpolationFactor);
            }
        }

        /// <summary>
        /// SyncVar Hook for the velocity update.
        /// </summary>
        /// <param name="realVelocity">The new velocity.</param>
        void UpdateVelocity(Vector3 realVelocity)
        {
            if (!hasAuthority)
            {
                this.realVelocity = vehicleRigidbody.velocity = realVelocity;
            }
        }

        /// <summary>
        /// SyncVar Hook for the angular velocity update.
        /// </summary>
        /// <param name="realAngularVelocity">The new angular velocity.</param>
        void UpdateAngularVelocity(Vector3 realAngularVelocity)
        {
            if (!hasAuthority)
            {
                this.realAngularVelocity = vehicleRigidbody.angularVelocity = realAngularVelocity;
            }
        }

        #endregion

        /// <summary>
        /// Called every physics tick to update car's position.
        /// </summary>
        void FixedUpdate()
        {
            if (hasAuthority)
            {
                realPosition = transform.position;
                realRotation = transform.rotation;
                CmdSynchroniseToServer(transform.position, transform.rotation, vehicleRigidbody.velocity,vehicleRigidbody.angularVelocity);
            }
        }

        /// <summary>
        /// A command to the server which updates the variables on the server. Updating the variables on the server cause
        /// variables on all clients to be synchronised.
        /// </summary>
        /// <param name="position">Car actual position.</param>
        /// <param name="rotation">Car actual rotation.</param>
        /// <param name="velocity">Car actual velocity.</param>
        [Command]
        void CmdSynchroniseToServer(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            UpdatePosition(position);
            UpdateRotation(rotation);
            UpdateVelocity(velocity);
            UpdateAngularVelocity(angularVelocity);
        }
    }
}