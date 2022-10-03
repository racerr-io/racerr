using NWH.VehiclePhysics;
using NWH.WheelController3D;
using Racerr.Utility;
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
        void Awake()
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
        public float Acceleration
        {
            get => vehicleController.engine.maxPower;
            set => vehicleController.engine.maxPower = value;
        }
        public float LowSpeedSteerAngle => vehicleController.steering.lowSpeedAngle;
        public float InputVertical
        {
            get => vehicleController.input.Vertical;
            set => vehicleController.input.Vertical = value;
        }

        /// <summary>
        /// Make car physics suitable for multiplayer. This should only be called on the other players cars
        /// on the current client.
        /// </summary>
        public void MakeSuitableForMultiplayerForEnemyCar()
        {
            foreach (WheelController wheelController in GetComponentsInChildren<WheelController>())
            {
                wheelController.enabled = false;
            }
        }

        /// <summary>
        /// Makes the car physics invulnerable, meaning the car cannot have collisions.
        /// </summary>
        /// <param name="durationSeconds">How long to stay invulnerable for.</param>
        public void SetInvulnerableTemporarily(int durationSeconds)
        {
            SetWheelsInvulnerableTemporarily(durationSeconds);
            SetAllLayersTemporarily(gameObject, LayerMask.NameToLayer(GameObjectIdentifiers.Invulnerable), durationSeconds);
        }

        /// <summary>
        /// Set wheels invulnerable, so they don't collide with other objects other than the road.
        /// </summary>
        /// <param name="durationSeconds">How long to stay invulnerable for.</param>
        void SetWheelsInvulnerableTemporarily(int durationSeconds)
        {
            // This weird bit shifting code was inspired by line 227-228 in WheelController.cs. I have no idea why (1 << 2) is neccessary.
            LayerMask invulnerableLayerMask = ~(LayerMask.GetMask(GameObjectIdentifiers.Invulnerable, GameObjectIdentifiers.IgnoreRaycast) | (1 << 2));
            LayerMask standardLayerMask = ~(LayerMask.GetMask(GameObjectIdentifiers.IgnoreRaycast) | (1 << 2));

            foreach (Wheel wheel in Wheels)
            {
                wheel.WheelController.ScanIgnoreLayers = invulnerableLayerMask;
            }

            this.YieldThenExecuteAsync(new WaitForSeconds(durationSeconds), () =>
            {
                foreach (Wheel wheel in Wheels)
                {
                    wheel.WheelController.ScanIgnoreLayers = standardLayerMask;
                }
            });
        }

        /// <summary>
        /// Set layer as invulnerable layer, so car passes through other cars. Layers are used in Unity to group various objects, similar to tags.
        /// The idea is that there is a special layer "Invulnerable" which passes through "Default" and "Invulnerable" layer. This gives it the effect
        /// of being able to drive through cars (assigned "Default" layer) but the car won't fall through the road (assigned "Static" layer).
        /// </summary>
        /// <param name="rootGameObject">Parent game object to set the layer.</param>
        /// <param name="newLayer">ID of the new layer.</param>
        /// <param name="durationSeconds">How long to stay invulnerable for.</param>
        void SetAllLayersTemporarily(GameObject rootGameObject, int newLayer, int durationSeconds)
        {
            int prevLayer = rootGameObject.layer;
            rootGameObject.layer = newLayer;

            foreach (Transform child in rootGameObject.transform)
            {
                SetAllLayersTemporarily(child.gameObject, newLayer, durationSeconds);
            }

            this.YieldThenExecuteAsync(new WaitForSeconds(durationSeconds), () =>
            {
                rootGameObject.layer = prevLayer;
            });
        }
    }
}
