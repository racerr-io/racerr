using Mirror;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Custom designed interpolation for car driving.
    /// </summary>
    public class RacerrCarNetworkTransform : NetworkBehaviour
    {
        [SyncVar] Vector3 RealPosition = Vector3.zero;
        [SyncVar] Quaternion RealRotation;
        [SyncVar] Vector3 RealVelocity;

        [SerializeField] [Range(0, 1)] float InterpolationFactor = 0.4f;
        Rigidbody Rigidbody { get; set; }

        /// <summary>
        /// Called when car is instantiated. If car is someone elses car update the rigidbody and remove the wheel colliders
        /// as they intefere with movement.
        /// </summary>
        void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();

            if (!isLocalPlayer)
            {
                Rigidbody.isKinematic = true;
                Rigidbody.useGravity = false;

                foreach (WheelCollider wheelCollider in GetComponentsInChildren<WheelCollider>())
                {
                    Destroy(wheelCollider);
                }
            }
        }

        /// <summary>
        /// Called every physics tick to update car's position.
        /// </summary>
        void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                RealPosition = transform.position;
                RealRotation = transform.rotation;
                RealVelocity = Rigidbody.velocity;
                CmdSynchroniseToServer(transform.position, transform.rotation, Rigidbody.velocity);
            }
            else
            {
                Vector3 predictedPosition = RealPosition + Time.deltaTime * RealVelocity; // Try to predict where the car might be. TODO: Incorporate difference in network time and local time.
                transform.position = Vector3.Lerp(transform.position, predictedPosition, InterpolationFactor);
                transform.rotation = Quaternion.Lerp(transform.rotation, RealRotation, InterpolationFactor);
                Rigidbody.velocity = RealVelocity;
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
        void CmdSynchroniseToServer(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            RealPosition = position;
            RealRotation = rotation;
            RealVelocity = velocity;
        }
    }
}