using NWH.VehiclePhysics;
using Racerr.Infrastructure.Client;
using Racerr.World.Track;
using UnityEngine;

namespace Racerr.UX.Camera
{
    public class BuildingTransparencyRaycaster : MonoBehaviour
    {
        /// <summary>
        /// Draws 4 rays towards the wheels of the car to collide with a building.
        /// If collides with building then building will update its state (i.e. become transparent).
        /// </summary>
        void Update()
        {
            if (ClientStateMachine.Singleton.LocalPlayer == null || ClientStateMachine.Singleton.LocalPlayer.CarManager == null)
            {
                return;
            }

            foreach (Wheel wheel in ClientStateMachine.Singleton.LocalPlayer.CarManager.Wheels)
            {
                Vector3 direction = wheel.ControllerTransform.position - transform.position;
                if (Physics.Raycast(transform.position, direction, out RaycastHit raycastHit))
                {
                    BuildingManager buildingState = raycastHit.collider.gameObject.GetComponent<BuildingManager>();
                    if (buildingState != null)
                    {
                        buildingState.HitRay();
                    }
                }
            }
        }
    }
}
