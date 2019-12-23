using UnityEngine;
using System.Collections;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Cruise control
    /// </summary>
    [System.Serializable]
    public class CruiseControl : DrivingAssists.DrivingAid
    {
        [Tooltip("Target speed in m/s. Can be both positive and negative.")]
        public float targetSpeed = 0f;

        [Tooltip("Shoud cruise control system deactivate when user input is detected (vertical axis)?")]
        public bool deactivateOnVerticalInput = true;

        [Tooltip("Should cruise control system use brakes if speed is too high? I set to false cruise control will only use throttle to regulate speed.")]
        public bool useBrakesOnOverspeed = true;

        [Tooltip("How hard should cruise control brake when overspeeding? 1 equals max brake torque set under Brakes dropdown.")]
        public float overspeedBrakeIntensity = 0.3f;

        private float correction;
        private float prevVerticalInput;
        private float speedDiff;
        private float vertical;


        public void Update(VehicleController vc)
        {
            if(deactivateOnVerticalInput && (vc.input.Vertical < 0.1f || vc.input.Vertical > 0.1f))
            {
                enabled = false;
            }

            speedDiff = vc.ForwardVelocity - targetSpeed;
            correction = -Mathf.Sign(speedDiff) * Mathf.Pow(speedDiff, 2f) * 0.5f;
            vertical = useBrakesOnOverspeed ? 
                Mathf.Clamp(correction, -1f, 0f) * overspeedBrakeIntensity + Mathf.Clamp(correction, 0f, 1f) 
                : Mathf.Clamp(correction, 0f, 1f);

            vc.input.Vertical = Mathf.Lerp(prevVerticalInput, vertical, Time.fixedDeltaTime * 10f);

            prevVerticalInput = vc.input.Vertical;
        }
    }
}
