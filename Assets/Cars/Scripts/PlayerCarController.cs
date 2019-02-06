using Mirror;
using Racerr.UX.Camera;
using Racerr.UX.HUD;
using System;
using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Car controller for all cars and players.
    /// </summary>
    public class PlayerCarController : NetworkBehaviour
    {
        [SerializeField] WheelCollider WheelFrontLeft, WheelFrontRight, WheelRearLeft, WheelRearRight;
        [SerializeField] Transform TransformFrontLeft, TransformFrontRight, TransformRearLeft, TransformRearRight;
        [SerializeField] float MaxSteerAngle = 10;
        [SerializeField] float MotorForce = 2500;
        [SerializeField] float Downforce = 7500;

        float HorizontalInput { get; set; }
        float VerticalInput { get; set; }
        float SteeringAngle { get; set; }
        int LastStiffness { get; set; } = 0;

        /// <summary>
        /// Called when car instantiated. Setup the user's view of the car.
        /// </summary>
        void Start()
        {
            if (isLocalPlayer)
            {
                FindObjectOfType<HUDSpeed>().Car = this;
                FindObjectOfType<AutoCam>().SetTarget(transform);
            }
        }

        /// <summary>
        /// Called every physics update. Drive the users car.
        /// </summary>
        void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                GetInput();
                Steer();
                Accelerate();
                UpdateWheelPositions();
                AddDownForce();
                UpdateSidewaysFrictionWithSpeed();
            }
        }

        /// <summary>
        /// Get input from users controls.
        /// TODO: Turn this into a function called Move() that takes in inputs and create a new script for User input
        /// so that AI can be decoupled.
        /// </summary>
        void GetInput()
        {
            HorizontalInput = Input.GetAxis("Horizontal");
            VerticalInput = Input.GetAxis("Vertical");
        }

        /// <summary>
        /// Steer the front wheels.
        /// </summary>
        void Steer()
        {
            SteeringAngle = MaxSteerAngle * HorizontalInput;
            WheelFrontLeft.steerAngle = SteeringAngle;
            WheelFrontRight.steerAngle = SteeringAngle;
        }

        /// <summary>
        /// Apply torque to wheels to accelerate. More torque means more speed.
        /// </summary>
        void Accelerate()
        {
            WheelRearLeft.motorTorque = VerticalInput * MotorForce;
            WheelRearRight.motorTorque = VerticalInput * MotorForce;
        }

        /// <summary>
        /// Make the wheel meshes match the state of the wheel colliders.
        /// </summary>
        void UpdateWheelPositions()
        {
            UpdateWheelPosition(WheelFrontLeft, TransformFrontLeft);
            UpdateWheelPosition(WheelFrontRight, TransformFrontRight);
            UpdateWheelPosition(WheelRearLeft, TransformRearLeft);
            UpdateWheelPosition(WheelRearRight, TransformRearRight);
        }

        /// <summary>
        /// Make the wheel mesh turn with the wheel collider.
        /// </summary>
        /// <param name="collider">The wheel collider</param>
        /// <param name="transform">The wheel mesh transform</param>
        void UpdateWheelPosition(WheelCollider collider, Transform transform)
        {
            Vector3 pos = transform.position;
            Quaternion quat = transform.rotation;

            collider.GetWorldPose(out pos, out quat);
            transform.position = pos;
            transform.rotation = quat;
        }

        /// <summary>
        /// Apply down force to avoid car flipping over.
        /// </summary>
        void AddDownForce()
        {
            Rigidbody carRigidBody = WheelFrontLeft.attachedRigidbody;
            carRigidBody.AddForce(-transform.up * Downforce * carRigidBody.velocity.magnitude);
        }

        /// <summary>
        /// Increase friction as speed increases. Useful to prevent slipping and shaking cars.
        /// </summary>
        void UpdateSidewaysFrictionWithSpeed()
        {
            Vector3 currentSpeed = WheelFrontLeft.attachedRigidbody.velocity;
            int stiffness = Convert.ToInt32(Mathf.Lerp(1, 5, currentSpeed.magnitude / 50));
            if (stiffness == LastStiffness)
            {
                return;
            }

            LastStiffness = stiffness;
            WheelFrictionCurve wheelFrictionCurve = WheelFrontLeft.sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            WheelFrontLeft.sidewaysFriction = wheelFrictionCurve;
            WheelFrontRight.sidewaysFriction = wheelFrictionCurve;
            WheelRearLeft.sidewaysFriction = wheelFrictionCurve;
            WheelRearRight.sidewaysFriction = wheelFrictionCurve;
        }
    }
}