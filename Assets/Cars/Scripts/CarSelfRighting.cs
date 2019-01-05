using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Automatically put the car the right way up, if it has come to rest upside-down.
    /// </summary>
    [RequireComponent(typeof (Rigidbody))]
    public class CarSelfRighting : MonoBehaviour
    {
        [SerializeField] float m_WaitTime = 3f;           // time to wait before self righting
        [SerializeField] float m_VelocityThreshold = 1f;  // the velocity below which the car is considered stationary for self-righting

        float LastOkTime { get; set; } // the last time that the car was in an OK state
        Rigidbody Rigidbody { get; set; }

        /// <summary>
        /// Find the rigidbody associated with the car
        /// </summary>
        void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Called every frame to check if the car is the right way up
        /// </summary>
        void Update()
        {
            if (transform.up.y > 0f || Rigidbody.velocity.magnitude > m_VelocityThreshold)
            {
                LastOkTime = Time.time;
            }

            if (Time.time > LastOkTime + m_WaitTime)
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
