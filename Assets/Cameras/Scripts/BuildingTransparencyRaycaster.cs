using Racerr.Car.Core;
using Racerr.StateMachine.Client;
using UnityEngine;

public class BuildingTransparencyRaycaster : MonoBehaviour
{
    /// <summary>
    /// Draws 4 rays towards the wheels of the car to collide with a building.
    /// If collides with building then building will update its state (i.e. become transparent).
    /// </summary>
    void Update()
    {
        PlayerCarController car = ClientStateMachine.Singleton.LocalPlayer?.Car;

        if (car != null)
        {
            foreach (Transform wheelTransform in car.WheelTransforms)
            {
                Vector3 direction = wheelTransform.position - transform.position;
                RaycastHit raycastHit;

                if (Physics.Raycast(transform.position, direction, out raycastHit))
                {
                    BuildingState buildingState = raycastHit.collider.gameObject.GetComponent<BuildingState>();

                    if (buildingState != null)
                    {
                        buildingState.HitRay();
                    }
                }
            }
        }
    }        
}

