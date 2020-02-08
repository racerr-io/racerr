using Racerr.Gameplay.Car;
using Racerr.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// A camera to automatically follow a target.
    /// </summary>
    [ExecuteInEditMode]
    public class PrimaryCamera : MonoBehaviour
    {
        [SerializeField] Vector3 overheadCamPosition;
        [SerializeField] Quaternion overheadCamRotation;
        [SerializeField] Vector3 thirdPersonCamPosition;
        [SerializeField] Quaternion thirdPersonCamRotation;
        [SerializeField] GameObject cam;
        [SerializeField] float moveSpeed = 3; // How fast the rig will move to keep up with target's position
        [SerializeField] Transform target;    // The target object to follow

        public enum CameraType
        {
            Overhead,
            ThirdPerson
        }
        CameraType camType = CameraType.Overhead;
        public CameraType CamType
        {
            get => camType;
            private set
            {
                camType = value;

                IEnumerable<PlayerBarConfiguration> configsToApply = FindObjectsOfType<PlayerBarConfiguration>().Where(config => config.CameraType == CamType);
                foreach (PlayerBarConfiguration config in configsToApply)
                {
                    config.ApplyConfiguration();
                }
            }
        }

        public Transform Target
        {
            get => target;
            set
            {
                target = value;

                if (target.CompareTag(GameObjectIdentifiers.Car))
                {
                    CamType = CameraType.ThirdPerson;
                } 
                else
                {
                    CamType = CameraType.Overhead;
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                CamType = (CameraType)(((int)CamType + 1) % Enum.GetNames(typeof(CameraType)).Length);
            }
        }

        /// <summary>
        /// Update camera after every physics tick.
        /// </summary>
        void FixedUpdate()
        {
            Vector3 newPosition;
            Quaternion newRotation;
            if (CamType == CameraType.Overhead)
            {
                newPosition = overheadCamPosition;
                newRotation = overheadCamRotation;
            } 
            else
            {
                newPosition = thirdPersonCamPosition;
                newRotation = thirdPersonCamRotation;
            }

            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, newPosition, Time.fixedDeltaTime * moveSpeed);
            cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, newRotation, Time.fixedDeltaTime * moveSpeed);

            FollowTarget();
        }

        /// <summary>
        /// Follow the camera's current transform target, by applying a smooth lerp transition.
        /// </summary>
        void FollowTarget()
        {
            // If no target, then we quit early as there is nothing to do (and to protect null references)
            if (target != null)
            {
                // Camera position moves towards target position
                transform.position = Vector3.Lerp(transform.position, target.position, Time.fixedDeltaTime * moveSpeed);

                if (CamType == CameraType.ThirdPerson)
                {
                    transform.position = target.transform.position - target.transform.forward;
                    transform.LookAt(target.transform.position);
                } 
                else
                {
                    transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}
