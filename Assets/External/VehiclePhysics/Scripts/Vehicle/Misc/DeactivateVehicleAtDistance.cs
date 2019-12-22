using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWH.WheelController3D;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Class to deactivate a vehicle when player exceeds set distance from it.
    /// Script will also deactivate any wheel controllers attached to it.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    [RequireComponent(typeof(Rigidbody))]
    public class DeactivateVehicleAtDistance : MonoBehaviour
    {
        [Tooltip("Distance at which vehicle will be deactivated. Set to 0 to disable.")]
        public float distance = 10f;
        public Transform player;

        private VehicleController vc;
        private WheelController[] wcs;
        private Rigidbody rb;


        private void Start()
        {
            vc = GetComponent<VehicleController>();
            wcs = vc.GetComponentsInChildren<WheelController>();
            rb = GetComponent<Rigidbody>();
        }


        void Update()
        {
            // Let vehicles run for a few frames to get them initialized and get them to settle
            if (Time.frameCount < 30) return;

            // Do not do anything if distance is 0 or if player is not active (presumed in vehicle)
            if (player == null || distance == 0 || !player.gameObject.activeSelf) return;

            float dst = Vector3.Distance(transform.position, player.position);

            // Disable
            if (dst > distance)
            {
                vc.enabled = false;
                foreach (WheelController wc in wcs)
                {
                    wc.enabled = false;
                }
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }
            // Enable
            else
            {
                if (!vc.enabled) vc.enabled = true;
                foreach (WheelController wc in wcs)
                {
                    wc.enabled = true;
                }
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }
        }
    }
}
