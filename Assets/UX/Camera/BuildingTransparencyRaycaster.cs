using NWH.VehiclePhysics;
using Racerr.Infrastructure.Client;
using Racerr.Utility;
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
            if (ClientStateMachine.Singleton.LocalPlayer == null || ClientStateMachine.Singleton.LocalPlayer.Car == null)
            {
                return;
            }

            foreach (Wheel wheel in ClientStateMachine.Singleton.LocalPlayer.Car.Physics.Wheels)
            {
                Vector3 direction = wheel.ControllerTransform.position - transform.position;
                if (Physics.Raycast(transform.position, direction, out RaycastHit raycastHit) 
                    && raycastHit.collider.gameObject.CompareTag(GameObjectIdentifiers.Environment))
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
