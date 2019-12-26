using Mirror;
using NWH.VehiclePhysics;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Manages the physics (NWH Vehicle Physics and Wheel Controllers) of the car.
    /// Provides a safe interface for other Racerr components to get information and modify
    /// functionality relating to the driving experience of the car.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class CarPhysicsManager : NetworkBehaviour
    {
        VehicleController vehicleController;

        /// <summary>
        /// Called on instantiation and caches the NWH Vehicle Controller.
        /// </summary>
        void Start()
        {
            vehicleController = GetComponent<VehicleController>();
        }

        public float SpeedKPH => vehicleController.SpeedKPH;

        /// <summary>
        /// Cars initially spawn in an inactive state, so they cannot be driven before the race starts.
        /// Once the race starts, this function is called by the server to allow all players in the race
        /// to drive their car.
        /// </summary>
        /// <param name="active">Whether the car should be active or not.</param>
        [ClientRpc]
        public void RpcSetActive(bool active)
        {
            vehicleController.Active = active;
        }
    }

}
