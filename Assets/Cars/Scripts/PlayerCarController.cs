using Mirror;
using Racerr.MultiplayerService;
using Racerr.RaceSessionManager;
using Racerr.Track;
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
        [SerializeField] WheelCollider wheelFrontLeft, wheelFrontRight, wheelRearLeft, wheelRearRight;
        [SerializeField] Transform transformFrontLeft, transformFrontRight, transformRearLeft, transformRearRight;
        [SerializeField] float maxSteerAngle = 10;
        [SerializeField] float motorForce = 2500;
        [SerializeField] float downforce = 7500;

        float HorizontalInput { get; set; }
        float VerticalInput { get; set; }
        float SteeringAngle { get; set; }
        int LastStiffness { get; set; } = 0;

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
        }

        public override void OnStartAuthority()
        {
            if (hasAuthority)
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
            if (hasAuthority)
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
        /// Detect if the car is moving through triggers.
        /// </summary>
        /// <param name="collider">The collider is went through.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == TrackPieceComponent.Checkpoint)
            {
                RacerrRaceSessionManager.Singleton.NotifyPlayerFinished(Player);
                Debug.Log("Hit a Track Piece Checkpoint!");
            }
            else if (collider.name == TrackPieceComponent.FinishLineCheckpoint)
            {
                RacerrRaceSessionManager.Singleton.NotifyPlayerFinished(Player);
                Debug.Log("Reached the finish line well done!!");
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
            SteeringAngle = maxSteerAngle * HorizontalInput;
            wheelFrontLeft.steerAngle = SteeringAngle;
            wheelFrontRight.steerAngle = SteeringAngle;
        }

        /// <summary>
        /// Apply torque to wheels to accelerate. More torque means more speed.
        /// </summary>
        void Accelerate()
        {
            wheelRearLeft.motorTorque = VerticalInput * motorForce;
            wheelRearRight.motorTorque = VerticalInput * motorForce;
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
            carRigidBody.AddForce(-transform.up * downforce * carRigidBody.velocity.magnitude);
        }

        /// <summary>
        /// Increase friction as speed increases. Useful to prevent slipping and shaking cars.
        /// </summary>
        void UpdateSidewaysFrictionWithSpeed()
        {
            Vector3 currentSpeed = wheelFrontLeft.attachedRigidbody.velocity;
            int stiffness = Convert.ToInt32(Mathf.Lerp(1, 5, currentSpeed.magnitude / 50));
            if (stiffness == LastStiffness)
            {
                return;
            }

            LastStiffness = stiffness;
            WheelFrictionCurve wheelFrictionCurve = wheelFrontLeft.sidewaysFriction;
            wheelFrictionCurve.stiffness = stiffness;

            wheelFrontLeft.sidewaysFriction = wheelFrictionCurve;
            wheelFrontRight.sidewaysFriction = wheelFrictionCurve;
            wheelRearLeft.sidewaysFriction = wheelFrictionCurve;
            wheelRearRight.sidewaysFriction = wheelFrictionCurve;
        }
    }
}