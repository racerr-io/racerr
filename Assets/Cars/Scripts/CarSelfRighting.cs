using Mirror;
using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Automatically put the car the right way up, if it has come to rest upside-down.
    /// </summary>
    [RequireComponent(typeof (Rigidbody))]
    public class CarSelfRighting : NetworkBehaviour
    {
        [SerializeField] float waitTime = 3f;           // time to wait before self righting
        [SerializeField] float velocityThreshold = 1f;  // the velocity below which the car is considered stationary for self-righting

        float lastOkTime; // the last time that the car was in an OK state
        new Rigidbody rigidbody;

        /// <summary>
        /// Find the rigidbody associated with the car
        /// </summary>
        void Start()
        {
            if (!hasAuthority)
            {
                Destroy(this);
            }
            rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Called every frame to check if the car is the right way up
        /// </summary>
        void Update()
        {
            if (transform.up.y > 0f || rigidbody.velocity.magnitude > velocityThreshold)
            {
                lastOkTime = Time.time;
            }

            if (Time.time > lastOkTime + waitTime)
            {
                RightCar();
            }
        }


        /// <summary>
        /// Put the car back the right way up.
        /// Set the correct orientation for the car, and lift it off the ground a little.
        /// </summary>
        void RightCar()
        {
            transform.position += Vector3.up;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
    }
}
