using Mirror;
using Racerr.UX.Camera;
using Racerr.UX.HUD;
using UnityEngine;

namespace Racerr.Car.Core
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    /// <summary>
    /// Primary Car Controller for any car.
    /// Attach to a car and call Move() to move the car.
    /// </summary>
    public class CarController : NetworkBehaviour
    {
        [Header("Primary Car Properties")]
        [Range(1, 20)] [SerializeField] float m_Acceleration = 1f;
        [Range(1, 20)] [SerializeField] float m_TopSpeed = 1f;
        [Range(1, 20)] [SerializeField] float m_Weight = 1f;
        [Range(1, 20)] [SerializeField] float m_HandlingNotImplemented;
        [Range(1, 20)] [SerializeField] float m_ArmourNotImplemented;
        [Range(1, 20)] [SerializeField] float m_AbilityNotImplemented;

        [Header("Other Car Properties")]
        [SerializeField] CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] Vector3 m_CentreOfMassOffset;
        [SerializeField] float m_MaximumSteerAngle;
        [Range(0, 1)] [SerializeField] float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)] [SerializeField] float m_TractionControl; // 0 is no traction control, 1 is full interference
        [Range(1000, 7500)] [SerializeField] float m_FullTorqueOverAllWheels = 2500f;
        [SerializeField] float m_ReverseTorque;
        [SerializeField] float m_MaxHandbrakeTorque;
        [SerializeField] float m_Downforce = 100f;
        [SerializeField] SpeedType m_SpeedType;
        [SerializeField] int m_NoOfGears = 5;
        [SerializeField] float m_RevRangeBoundary = 1f;
        [SerializeField] int m_RPMUpperBound;
        [SerializeField] float m_SlipLimit;
        [SerializeField] float m_BrakeTorque;

        Quaternion[] WheelMeshLocalRotations { get; set; }
        Vector3 PrevPos { get; set; }
        Vector3 Pos { get; set; }
        int GearNum { get; set; }
        float GearFactor { get; set; }
        float OldRotation { get; set; }
        float CurrentTorque { get; set; }
        Rigidbody Rigidbody { get; set; }
        const float ReversingThreshold = 0.01f;

        public float AccelInput { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get; private set; }
        public float MaxSpeed => m_TopSpeed; 
        public float Revs { get; private set; } // Some decimal value between 0 and 1
        public int CurrentRPM => (int)(Revs * m_RPMUpperBound) + m_RPMUpperBound/8 + Random.Range(-5, 5);
        public float CurrentSpeed => m_SpeedType == SpeedType.MPH ? Rigidbody.velocity.magnitude * 2.23693629f : Rigidbody.velocity.magnitude * 3.6f;
        public string SpeedTypeMetric => m_SpeedType.ToString();

        /// <summary>
        /// Initialise wheel colliders, acquire the rigidbody of the car and calculate the current torque.
        /// </summary>
        void Start()
        {
            WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

            m_MaxHandbrakeTorque = float.MaxValue;

            Rigidbody = GetComponent<Rigidbody>();
            SetupCarStats();
            CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl*m_FullTorqueOverAllWheels);

            if (isLocalPlayer)
            {
                FindObjectOfType<HUDRPM>().Car = this;
              //  FindObjectOfType<HUDSpeed>().Car = this;
                FindObjectOfType<AutoCam>().SetTarget(transform);
            }
        }

        /// <summary>
        /// Using the basic abstracted car stats which are out of 20 (acceleration, handling, etc), 
        /// adjust and scale specific car properties to meet the requirements.
        /// </summary>
        void SetupCarStats()
        {
            // Acceleration
            m_FullTorqueOverAllWheels += 180 * m_Acceleration;

            // Top Speed
            m_TopSpeed = 150 + 12 * m_TopSpeed;

            // Weight
            Rigidbody.mass = 500 + 50 * m_Weight;
        }

        /// <summary>
        /// Handle cases where the car hit something.
        /// E.g. Checkpoints, Spike Strips, Walls, Other cars etc.
        /// You can grab the collided game object using collider.gameObject.
        /// </summary>
        /// <param name="collider">The collider it hit</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.name == "Checkpoint") // A normal checkpoint
            {
                Debug.Log("Car hit checkpoint!");
            }
            else if (collider.gameObject.name == "Finish Line Checkpoint") // Last checkpoint - end of race
            {
                Debug.Log("Car reached the end of the track!");
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// At some instance, change the gear one level up or down, depending on the speed of the car.
        /// </summary>
        void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) m_NoOfGears)*(GearNum + 1);
            float downgearlimit = (1/(float) m_NoOfGears)*GearNum;

            if (GearNum > 0 && f < downgearlimit)
            {
                GearNum--;
            }

            if (f > upgearlimit && (GearNum < (m_NoOfGears - 1)))
            {
                GearNum++;
            }
        }

        /// <summary>
        /// Simple function to add a curved bias towards 1 for a value in the 0-1 range
        /// </summary>
        /// <param name="factor"></param>
        /// <returns>Value with the bias</returns>
        float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }


        /// <summary>
        /// Unclamped version of Lerp, to allow value to exceed the from-to range
        /// </summary>
        /// <param name="from">Lower bound</param>
        /// <param name="to">Upper bound</param>
        /// <param name="value">Value to Lerp</param>
        /// <returns>Unclamped Lerp Value</returns>
        float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }

        /// <summary>
        /// Gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        /// We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        /// </summary>
        void CalculateGearFactor()
        {
            float f = (1/(float) m_NoOfGears);
            float targetGearFactor = Mathf.InverseLerp(f*GearNum, f*(GearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            GearFactor = Mathf.Lerp(GearFactor, targetGearFactor, Time.deltaTime*5f);
        }


        /// <summary>
        /// calculate engine revs (for display / sound)
        /// (this is done in retrospect - revs are not used in force/power calculations)
        /// </summary>
        void CalculateRevs()
        {
            CalculateGearFactor();
            float gearNumFactor = GearNum/(float) m_NoOfGears;
            float revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            float revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, GearFactor);
        }

        /// <summary>
        /// Move the car. Please note that the inputs are analog - if using a joystick or gyroscope then the amount you move
        /// the stick/device will affect steering/acceleration/footbrake/handbrake pressure (like a real car!).
        /// </summary>
        /// <param name="steering">Steering input, where negative is left and positive is right.</param>
        /// <param name="accel">Acceleration input, where negative is negative acceleration and positive is positive acceleration.</param>
        /// <param name="footbrake">The normal car brake - apply reverse torque when value is positive.</param>
        /// <param name="handbrake">Handbrake - Handbrake on back wheels when value is positive.</param>
        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;

                if (accel > 0)
                {
                    m_WheelEffects[i].EmitSmoke();
                }
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1*Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            CurrentSteerAngle = steering*m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = CurrentSteerAngle;
            m_WheelColliders[1].steerAngle = CurrentSteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                float hbTorque = handbrake*m_MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();

            m_WheelEffects[0].PlayAudio();
        }

        /// <summary>
        /// Instantly multiply the velocity of the car by the multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier value.</param>
        public void MultiplySpeed(float multiplier)
        {
            Rigidbody.velocity *= multiplier;
        }

        /// <summary>
        /// Limit the speed of the car depending on the top speed.
        /// </summary>
        void CapSpeed()
        {
            float speed = Rigidbody.velocity.magnitude;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > m_TopSpeed)
                        Rigidbody.velocity = (m_TopSpeed/2.23693629f) * Rigidbody.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > m_TopSpeed)
                        Rigidbody.velocity = (m_TopSpeed/3.6f) * Rigidbody.velocity.normalized;
                    break;
            }
        }

        /// <summary>
        /// Move the car forward and apply torque on the wheels.
        /// Apply reverse torque on wheels if braking.
        /// </summary>
        /// <remarks>Need to limit RPM to avoid the wheel spinning forever and car not stopping.</remarks>
        /// <param name="accel">Value to accelerate by.</param>
        /// <param name="footbrake">Whether the car is braking.</param>
        void ApplyDrive(float accel, float footbrake)
        {
            float thrustTorque;
            float torqueToApply;

            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (CurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        torqueToApply = m_WheelColliders[i].rpm < 2000 ? thrustTorque : 0;
                        m_WheelColliders[i].motorTorque = torqueToApply;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (CurrentTorque / 2f);
                    torqueToApply = m_WheelColliders[0].rpm < 2000 ? thrustTorque : 0;
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = torqueToApply;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (CurrentTorque / 2f);
                    torqueToApply = m_WheelColliders[2].rpm < 2000 ? thrustTorque : 0;
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = torqueToApply;
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, Rigidbody.velocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
                }
            }
        }

        /// <summary>
        /// Steering Assist
        /// </summary>
        void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(OldRotation - transform.eulerAngles.y) < 10f)
            {
                float turnadjust = (transform.eulerAngles.y - OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                Rigidbody.velocity = velRotation * Rigidbody.velocity;
            }
            OldRotation = transform.eulerAngles.y;
        }


        /// <summary>
        /// Add more grip in relation to speed
        /// </summary>
        void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up*m_Downforce*
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }

        /// <summary>
        /// Checks if the wheels are spinning and is so does three things
        /// 1) emits particles
        /// 2) plays tiure skidding sounds
        /// 3) leaves skidmarks on the ground
        /// these effects are controlled through the WheelEffects class
        /// </summary>
        void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
                {
                    m_WheelEffects[i].EmitSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }

        /// <summary>
        /// Crude traction control that reduces the power to wheel if the car is wheel spinning too much
        /// </summary>
        void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }

        /// <summary>
        /// Adjust power to wheels.
        /// </summary>
        /// <param name="forwardSlip">How much the wheel is slipping.</param>
        void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && CurrentTorque >= 0)
            {
                CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                CurrentTorque += 10 * m_TractionControl;
                if (CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }

        /// <summary>
        /// Any skid sound is playing at the moment?
        /// </summary>
        /// <returns>True or False depending on whether the sound is playing.</returns>
        bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
