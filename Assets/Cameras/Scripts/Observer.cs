using Racerr.Car.Core;
using Racerr.MultiplayerService;
using UnityEngine;

public class Observer : MonoBehaviour
{
    void Update()
    {
        PlayerCarController car = Player.LocalPlayer?.Car;

        if (car != null)
        {
            Vector3 direction = Player.LocalPlayer.Car.transform.position - Camera.main.transform.position;
            RaycastHit raycastHit;
            if (Physics.Raycast(Camera.main.transform.position, direction, out raycastHit))
            {
                BuildingState state = raycastHit.collider.gameObject.GetComponent<BuildingState>();
                if (state != null)
                {
                    state.HitRay();
                }
            }
        }
    }        
}

