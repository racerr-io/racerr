using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Client;
using Racerr.Track;
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
            CarController car = ClientStateMachine.Singleton.LocalPlayer?.Car;

            if (car != null)
            {
                foreach (Transform wheelTransform in car.WheelTransforms)
                {
                    Vector3 direction = wheelTransform.position - transform.position;
                    RaycastHit raycastHit;

                    if (Physics.Raycast(transform.position, direction, out raycastHit))
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
}
