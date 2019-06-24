﻿using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using Racerr.UX.Camera;
using Racerr.UX.Car;
using Racerr.UX.HUD;
using System;
using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Car controller for all cars in Racerr.
    /// </summary>
    public class PlayerCarController : NetworkBehaviour
    {
        [Header("Car Properties")]
        [SerializeField] float lowSpeedConstantSteeringAngle = 30;
        [SerializeField] float velocityAffected50SpeedSteeringAngle = 280;
        [SerializeField] float constant50SpeedSteeringAngle = 2;
        [SerializeField] float velocityAffectedSteeringAngle = 300;
        [SerializeField] float downforceWithLessThanFourWheels = 1875;
        [SerializeField] float downforceWithFourWheels = 7500;
        [SerializeField] float motorForce = 4000;
        [SerializeField] float brakeForce = 10000;
        [SerializeField] int maxHealth = 100;
        [SerializeField] int stiffnessNormal = 1;
        [SerializeField] int stiffnessBraking = 3;
        [SerializeField] WheelCollider wheelFrontLeft, wheelFrontRight, wheelRearLeft, wheelRearRight;
        [SerializeField] Transform transformFrontLeft, transformFrontRight, transformRearLeft, transformRearRight;
        public int MaxHealth => maxHealth;

        [Header("Player Bar Properties")]
        [SerializeField] GameObject playerBarPrefab;
        [SerializeField] float playerBarStartDisplacement = 4; // Displacement from car centre at all times
        [SerializeField] float playerBarUpDisplacement = 1; // Additional displacement when car is moving south of the screen (need this due to camera angle changes)
        public PlayerBar PlayerBar { get; private set; }
        public float PlayerBarStartDisplacement => playerBarStartDisplacement;
        public float PlayerBarUpDisplacement => playerBarUpDisplacement;

        float horizontalInput;
        float verticalInput;
        int lastStiffness = 0;
        new Rigidbody rigidbody;
        public bool IsAcceleratingBackwards => verticalInput < 0;
        public Transform[] WheelTransforms => new[] { transformFrontLeft, transformFrontRight, transformRearLeft, transformRearRight };
        public int Velocity => Convert.ToInt32(rigidbody.velocity.magnitude * 2);

        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set => playerGO = value;
        }

        public Player Player { get; private set; }

        /// <summary>
        /// Called when car instantiated. Setup the user's view of the car.
        /// </summary>
        void Start()
        {
            Player = PlayerGO.GetComponent<Player>();
            rigidbody = GetComponent<Rigidbody>();

            if (hasAuthority)
            {
                FindObjectOfType<HUDSpeed>().Car = this;
                FindObjectOfType<AutoCam>().SetTarget(transform);
            }

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.Car = this;
        }

        /// <summary>
        /// Called every physics update. Drive the users car.
        /// </summary>
        void FixedUpdate()
        {
            if (hasAuthority)
            {
                GetInput();
                Steer();
                Drive();
                UpdateWheelPositions();
                AddDownForce();
                UpdateSidewaysFrictionWithSpeed();
            }
        }

        /// <summary>
        /// Detect if the car is moving through triggers.
        /// </summary>
        /// <param name="collider">The collider that it went through.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == TrackPieceComponent.FinishLineCheckpoint || collider.name == TrackPieceComponent.Checkpoint)
            {
                RacerrRaceSessionManager.Singleton.NotifyPlayerPassedThroughCheckpoint(Player, collider.gameObject);
            }
        }

        /// <summary>
        /// Apply damage to car on collision with other players and world objects.
        /// </summary>
        /// <param name="collision">Collision information</param>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Environment"))
            {
                Player.Health -= 10;
            }
        }

        /// <summary>
        /// Get input from users controls.
        /// TODO: Turn this into a function called Move() that takes in inputs and create a new script for User input
        /// so that AI can be decoupled.
        /// </summary>
        void GetInput()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }

        /// <summary>
        /// Steer the front wheels.
        /// </summary>
        void Steer()
        {
            float steeringAngle = CalculateSteeringAngle() * horizontalInput;
            
            wheelFrontLeft.steerAngle = steeringAngle;
            wheelFrontRight.steerAngle = steeringAngle;
        }

        /// <summary>
        /// Returns steering angle depending on velocity.
        /// <remarks>
        /// For slower speeds, we have a constant steering angle to prevent insane steering angles and division by 0.
        /// For higher speeds, divide by velocity to reduce the steering angle at higher speeds. Also has the effect
        /// of a reduced growth rate of steering angle limit at higher speeds.
        /// </remarks>
        /// </summary>
        float CalculateSteeringAngle()
        {
            float steeringAngle;

            if (Velocity <= 10)
            {
                steeringAngle = lowSpeedConstantSteeringAngle;
            }
            else if (Velocity <= 40)
            {
                steeringAngle = velocityAffected50SpeedSteeringAngle / Velocity + constant50SpeedSteeringAngle;
            }
            else
            {
                steeringAngle = velocityAffectedSteeringAngle / Velocity;
            }

            return steeringAngle;
        }

        /// <summary>
        /// Accelerate/Decelerate normally if player is going forward/reversing. Else apply brakes.
        /// </summary>
        void Drive()
        {
            Vector3 localVel = transform.InverseTransformDirection(rigidbody.velocity);

            if (verticalInput >= 0 || (localVel.z < 0 && verticalInput <= 0))
            {
                Accelerate();
            }
            else
            {
                Brake();
            }
        }

        /// <summary>
        /// Apply torque to wheels to accelerate or reverse.
        /// </summary>
        void Accelerate()
        {
            if (wheelFrontLeft.forwardFriction != stiffnessNormal)
            {
                WheelFrictionCurve wheelFrictionCurve = wheelFrontLeft.forwardFriction;
                wheelFrictionCurve.stiffness = stiffnessNormal;

                wheelRearLeft.forwardFriction = wheelFrictionCurve;
                wheelRearRight.forwardFriction = wheelFrictionCurve;
            }
            wheelRearRight.motorTorque = verticalInput * motorForce;
            wheelRearLeft.motorTorque = verticalInput * motorForce;
            wheelFrontRight.motorTorque = 0;
            wheelFrontLeft.motorTorque = 0;
            wheelRearRight.brakeTorque = 0;
            wheelRearLeft.brakeTorque = 0;
            }

        /// <summary>
        /// Apply brakes + inverse acceleration and increase friction of wheels
        /// </summary>
        void Brake()
        {
            if (wheelFrontLeft.forwardFriction != stiffnessBraking)
            {
                WheelFrictionCurve wheelFrictionCurve = wheelFrontLeft.forwardFriction;
                wheelFrictionCurve.stiffness = stiffnessBraking;

                wheelRearLeft.forwardFriction = wheelFrictionCurve;
                wheelRearRight.forwardFriction = wheelFrictionCurve;
            }
            wheelFrontRight.motorTorque = verticalInput * motorForce;
            wheelFrontLeft.motorTorque = verticalInput * motorForce;
            wheelRearRight.brakeTorque = brakeForce;
            wheelRearLeft.brakeTorque = brakeForce;
            }

        /// <summary>
        /// Make the wheel meshes match the state of the wheel colliders.
        /// </summary>
        void UpdateWheelPositions()
        {
            UpdateWheelPosition(wheelFrontLeft, transformFrontLeft);
            UpdateWheelPosition(wheelFrontRight, transformFrontRight);
            UpdateWheelPosition(wheelRearLeft, transformRearLeft);
            UpdateWheelPosition(wheelRearRight, transformRearRight);
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
            Rigidbody carRigidBody = wheelFrontLeft.attachedRigidbody;
            Vector3 force;

            if (GetNumWheelsTouchingGround() >= 3)
            {
                force = Vector3.down * downforceWithFourWheels * carRigidBody.velocity.magnitude;
            }
            else
            {
                force = Vector3.down * downforceWithLessThanFourWheels * carRigidBody.velocity.magnitude;
            }

            carRigidBody.AddForce(force);
        }

        /// <summary>
        /// Determines the number of wheels colliding with something (i.e. touching the ground)
        /// </summary>
        /// <returns>Wheel collision count</returns>
        int GetNumWheelsTouchingGround()
        {
            int count = 0;

            if (wheelFrontLeft.GetGroundHit(out _))
            {
                count++;
            }

            if (wheelFrontRight.GetGroundHit(out _))
            {
                count++;
            }

            if (wheelRearLeft.GetGroundHit(out _))
            {
                count++;
            }

            if (wheelRearRight.GetGroundHit(out _))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Increase friction as speed increases. Useful to prevent slipping and shaking cars.
        /// </summary>
        void UpdateSidewaysFrictionWithSpeed()
        {
            Vector3 currentSpeed = wheelFrontLeft.attachedRigidbody.velocity;
            int stiffness = Convert.ToInt32(Mathf.Lerp(1, 5, currentSpeed.magnitude / 50));
            if (stiffness == lastStiffness)
            {
                return;
            }

            lastStiffness = stiffness;
            WheelFrictionCurve wheelFrictionCurve = wheelFrontLeft.sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            wheelFrontLeft.sidewaysFriction = wheelFrictionCurve;
            wheelFrontRight.sidewaysFriction = wheelFrictionCurve;
            wheelRearLeft.sidewaysFriction = wheelFrictionCurve;
            wheelRearRight.sidewaysFriction = wheelFrictionCurve;
        }
    }
}
