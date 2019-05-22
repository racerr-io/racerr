using Mirror;
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
        [SerializeField] float slowSpeedSteeringAngle = 15;
        [SerializeField] float velocityAffectedSteeringAngle = 100;
        [SerializeField] float constantSteeringAngle = 5;
        [SerializeField] float motorForce = 4000;
        [SerializeField] float downforce = 7500;
        [SerializeField] WheelCollider wheelFrontLeft, wheelFrontRight, wheelRearLeft, wheelRearRight;
        [SerializeField] Transform transformFrontLeft, transformFrontRight, transformRearLeft, transformRearRight;

        [Header("Player Bar Properties")]
        [SerializeField] GameObject playerBarPrefab;
        [SerializeField] float playerBarStartDisplacement = 4; // Displacement from car centre at all times
        [SerializeField] float playerBarUpDisplacement = 1; // Additional displacement when car is moving south of the screen (need this due to camera angle changes)
        public float PlayerBarStartDisplacement => playerBarStartDisplacement;
        public float PlayerBarUpDisplacement => playerBarUpDisplacement;

        float horizontalInput;
        float verticalInput;
        float steeringAngle;
        int lastStiffness = 0;
        public bool IsAcceleratingBackwards => verticalInput < 0;

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

            if (hasAuthority)
            {
                FindObjectOfType<HUDSpeed>().Car = this;
                FindObjectOfType<AutoCam>().SetTarget(transform);
            }

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar playerBar = PlayerBarGO.GetComponent<PlayerBar>();
            playerBar.Car = this;
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
        /// <param name="collider">The collider that it went through.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == TrackPieceComponent.FinishLineCheckpoint)
            {
                RacerrRaceSessionManager.Singleton.NotifyPlayerFinished(Player);
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
            steeringAngle = SteeringAngle(Convert.ToInt32(GetComponent<Rigidbody>().velocity.magnitude * 2)) * horizontalInput;
            
            wheelFrontLeft.steerAngle = steeringAngle;
            wheelFrontRight.steerAngle = steeringAngle;
        }

        /// <summary>
        /// Returns steeringAngle depending on velocity.
        /// </summary>
        float SteeringAngle(int velocity)
        {
            float steeringAngle1;

            if (velocity <= 10)
            {
                steeringAngle1 = slowSpeedSteeringAngle;
            }
            else
            {
                steeringAngle1 = velocityAffectedSteeringAngle / velocity + constantSteeringAngle;
            }
            Debug.Log("vel" + velocity);
            Debug.Log("slow" + slowSpeedSteeringAngle);
            Debug.Log("velaff" + velocityAffectedSteeringAngle);
            return steeringAngle1;
        }

        /// <summary>
        /// Apply torque to wheels to accelerate. More torque means more speed.
        /// </summary>
        void Accelerate()
        {
            wheelRearLeft.motorTorque = verticalInput * motorForce;
            wheelRearRight.motorTorque = verticalInput * motorForce;
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
            if (GetNumWheelsTouchingGround() >= 3)
            {
                Rigidbody carRigidBody = wheelFrontLeft.attachedRigidbody;
                carRigidBody.AddForce(-transform.up * downforce * carRigidBody.velocity.magnitude);
            }
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
