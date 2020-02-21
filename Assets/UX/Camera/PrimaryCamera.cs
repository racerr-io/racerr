using Racerr.Gameplay.Car;
using Racerr.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// The main camera of the game, which is always displayed.
    /// Follows a target.
    /// </summary>
    [ExecuteInEditMode]
    public class PrimaryCamera : MonoBehaviour
    {
        [SerializeField] Vector3 overheadCamPosition;
        [SerializeField] Quaternion overheadCamRotation;
        [SerializeField] Vector3 thirdPersonCamPosition;
        [SerializeField] Quaternion thirdPersonCamRotation;
        [SerializeField] Vector3 deathCamPosition;
        [SerializeField] Quaternion deathCamRotation;
        [SerializeField] GameObject cam;
        [SerializeField] BuildingTransparencyRaycaster buildingTransparencyRaycaster; 
        [SerializeField] float moveSpeed = 3;
        [SerializeField] Transform target;

        public enum CameraType
        {
            Overhead,
            ThirdPerson,
            Death
        }
        CameraType camType = CameraType.Overhead;
        public CameraType CamType
        {
            get => camType;
            private set
            {
                camType = value;

                this.AsyncYieldThenExecute(new WaitForFixedUpdate(), () =>
                {
                    // Each car has various configurations for their PlayerBar, because we want to vary the position of the PlayerBar depending on the
                    // size of the car and position of the camera. Upon changing the CameraType, activate the correct configuration on all cars.
                    IEnumerable<PlayerBarConfiguration> configsToApply = FindObjectsOfType<PlayerBarConfiguration>().Where(config => config.CameraType == CamType);
                    foreach (PlayerBarConfiguration config in configsToApply)
                    {
                        config.ApplyConfiguration();
                    }
                });

                // Only need the raycaster in overhead mode because buildings can block cars.
                buildingTransparencyRaycaster.enabled = camType == CameraType.Overhead;
            }
        }

        /// <summary>
        /// Set the target and the CameraType.
        /// </summary>
        /// <param name="target">Target transform to follow.</param>
        /// <param name="camType">CameraType, selected from the enum.</param>
        public void SetTarget(Transform target, CameraType camType)
        {
            this.target = target;
            CamType = camType;
        }

        /// <summary>
        /// Called every frame. Upon presing the button, we can
        /// cycle through all the values in the CameraType enum
        /// so the user can switch between cameras.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                do
                {
                    CamType = (CameraType)(((int)CamType + 1) % Enum.GetNames(typeof(CameraType)).Length);
                } while (CamType == CameraType.Death); // Don't want the user to be able to cycle to the death cam.
            }
        }

        /// <summary>
        /// Update camera after every physics tick, to move the camera and follow the target.
        /// </summary>
        void FixedUpdate()
        {
            UpdateCameraPosition();
            FollowTarget();
        }

        /// <summary>
        /// Smoothly lerp the camera to the correct position depending on the camera type.
        /// This provides a smooth transition for the user when the CameraType is changed by
        /// the game or the user.
        /// </summary>
        void UpdateCameraPosition()
        {
            Vector3 newPosition;
            Quaternion newRotation;
            if (CamType == CameraType.Overhead)
            {
                newPosition = overheadCamPosition;
                newRotation = overheadCamRotation;
            }
            else if (CamType == CameraType.Death)
            {
                newPosition = deathCamPosition;
                newRotation = deathCamRotation;
            }
            else
            {
                newPosition = thirdPersonCamPosition;
                newRotation = thirdPersonCamRotation;
            }

            // Note we are placing the entire camera GameObject inside the player's car in FollowTarget(), 
            // then we offset the position of the actual camera here.
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, newPosition, Time.fixedDeltaTime * moveSpeed);
            cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, newRotation, Time.fixedDeltaTime * moveSpeed);
        }

        /// <summary>
        /// Follow the camera's current transform target, by applying a smooth lerp transition.
        /// Note we are placing the entire camera GameObject inside the player's car,
        /// then we offset the position of the actual camera in UpdateCameraPosition().
        /// </summary>
        void FollowTarget()
        {
            // If no target, then we quit early as there is nothing to do (and to protect null references)
            if (target != null)
            {
                // Camera position moves towards target position
                transform.position = Vector3.Lerp(transform.position, target.position, Time.fixedDeltaTime * moveSpeed);

                if (CamType == CameraType.ThirdPerson || CamType == CameraType.Death)
                {
                    // In third person / death cam mode, position the camera behind the Player's car always.
                    transform.position = target.transform.position - target.transform.forward;
                    transform.LookAt(target.transform.position);
                } 
                else
                {
                    // In overhead mode, the rotation stays constant.
                    transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}
