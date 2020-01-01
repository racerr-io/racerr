using NWH.VehiclePhysics;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Manages the physics (NWH Vehicle Physics and Wheel Controllers) of the car.
    /// Provides a safe interface for other Racerr components to get information and modify
    /// functionality relating to the driving experience of the car.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class CarPhysicsManager : MonoBehaviour
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
        public bool IsActive
        {
            get => vehicleController.Active;
            set => vehicleController.Active = value;
        }
        public List<Wheel> Wheels => vehicleController.Wheels;
    }
}
