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
        [SerializeField] WheelCollider m_WheelFrontLeft, m_WheelFrontRight, m_WheelRearLeft, m_WheelRearRight;
        [SerializeField] Transform m_TransformFrontLeft, m_TransformFrontRight, m_TransformRearLeft, m_TransformRearRight;
        [SerializeField] float m_MaxSteerAngle = 10;
        [SerializeField] float m_MotorForce = 2500;
        [SerializeField] float m_Downforce = 7500;

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
            SteeringAngle = m_MaxSteerAngle * HorizontalInput;
            m_WheelFrontLeft.steerAngle = SteeringAngle;
            m_WheelFrontRight.steerAngle = SteeringAngle;
        }

        /// <summary>
        /// Apply torque to wheels to accelerate. More torque means more speed.
        /// </summary>
        void Accelerate()
        {
            m_WheelRearLeft.motorTorque = VerticalInput * m_MotorForce;
            m_WheelRearRight.motorTorque = VerticalInput * m_MotorForce;
        }

        /// <summary>
        /// Make the wheel meshes match the state of the wheel colliders.
        /// </summary>
        void UpdateWheelPositions()
        {
            UpdateWheelPosition(m_WheelFrontLeft, m_TransformFrontLeft);
            UpdateWheelPosition(m_WheelFrontRight, m_TransformFrontRight);
            UpdateWheelPosition(m_WheelRearLeft, m_TransformRearLeft);
            UpdateWheelPosition(m_WheelRearRight, m_TransformRearRight);
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
            Rigidbody carRigidBody = m_WheelFrontLeft.attachedRigidbody;
            carRigidBody.AddForce(-transform.up * m_Downforce * carRigidBody.velocity.magnitude);
        }

        /// <summary>
        /// Increase friction as speed increases. Useful to prevent slipping and shaking cars.
        /// </summary>
        void UpdateSidewaysFrictionWithSpeed()
        {
            Vector3 currentSpeed = m_WheelFrontLeft.attachedRigidbody.velocity;
            int stiffness = Convert.ToInt32(Mathf.Lerp(1, 5, currentSpeed.magnitude / 50));
            if (stiffness == LastStiffness)
            {
                return;
            }

            LastStiffness = stiffness;
            WheelFrictionCurve wheelFrictionCurve = m_WheelFrontLeft.sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            m_WheelFrontLeft.sidewaysFriction = wheelFrictionCurve;
            m_WheelFrontRight.sidewaysFriction = wheelFrictionCurve;
            m_WheelRearLeft.sidewaysFriction = wheelFrictionCurve;
            m_WheelRearRight.sidewaysFriction = wheelFrictionCurve;
        }
    }
}