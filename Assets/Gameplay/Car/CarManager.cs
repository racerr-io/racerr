using Mirror;
using NWH.VehiclePhysics;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Client;
using Racerr.Utility;
using Racerr.UX.Car;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Car Manager for all cars in Racerr.
    /// Adds Racerr specific customisation to the vehicle, such as health and the player bar.
    /// </summary>
    [RequireComponent(typeof(CarPhysicsManager))]
    public class CarManager : NetworkBehaviour
    {
        public Player OwnPlayer { get; private set; }

        [Header("Car Properties")]
        [SerializeField] int maxHealth = 100;
        const double healthDamageAdjustmentFactor = 0.00002f;
        public int MaxHealth => maxHealth;
        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set => playerGO = value;
        }
        CarPhysicsManager carPhysicsManager;
        public float SpeedKPH => carPhysicsManager.SpeedKPH;
        public List<Wheel> Wheels => carPhysicsManager.Wheels;

        [Header("Player Bar Properties")]
        [SerializeField] GameObject playerBarPrefab;
        [SerializeField] float playerBarStartDisplacement = 4; // Displacement from car centre at all times
        [SerializeField] float playerBarUpDisplacement = 1; // Additional displacement when car is moving south of the screen (need this due to camera angle changes)
        public PlayerBar PlayerBar { get; private set; }
        public float PlayerBarStartDisplacement => playerBarStartDisplacement;
        public float PlayerBarUpDisplacement => playerBarUpDisplacement;

        /// <summary>
        /// Called when the car is instantiated. Caches various fields for later use
        /// and instantiates the Player's Bar, which should appear above the car in the game.
        /// </summary>
        void Start()
        {
            OwnPlayer = PlayerGO.GetComponent<Player>();
            carPhysicsManager = GetComponent<CarPhysicsManager>();

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.CarManager = this;
        }

        /// <summary>
        /// Apply damage to car on collision with another player's front of the car, the environment (e.g. buildings)
        /// or if the collision is between the back of the two cars, by decreasing the players health by an amount proportional 
        /// to the force of the collision.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        void OnCollisionEnter(Collision collision)
        {
            Vector3 collisionForce = collision.impulse / Time.fixedDeltaTime;
            ContactPoint contactPoint = collision.GetContact(0);
            if (collision.gameObject.CompareTag(Tags.Environment) || contactPoint.otherCollider.gameObject.CompareTag(Tags.CarFrontCollider)
                || (contactPoint.thisCollider.gameObject.CompareTag(Tags.CarBackCollider) && contactPoint.otherCollider.gameObject.CompareTag(Tags.CarBackCollider)))
            {
                OwnPlayer.Health -= Convert.ToInt32(collisionForce.magnitude * healthDamageAdjustmentFactor);
            }
        }

        /// <summary>
        /// Cars initially spawn in an inactive state, so they cannot be driven before the race starts.
        /// Once the race starts, this function is called by the server to allow all players in the race
        /// to drive their car. The client will then call this again to deactivate themself if they die,
        /// since we leave their corpse on the track and we don't want them to be driving it.
        /// </summary>
        /// <param name="isActive">Whether the car should be active or not.</param>
        public void SetIsActive(bool isActive)
        {
            // Need to check if a Host (when you use the game in the editor as both client and server) is trying to change its own
            // active status. In this case, we do not want to call the RPC and cause an infinite loop.
            bool isHostTargetingItself = ClientStateMachine.Singleton != null && ClientStateMachine.Singleton.LocalPlayer == OwnPlayer;
            
            if (isServer && !isHostTargetingItself)
            {
                TargetSetIsActive(OwnPlayer.connectionToClient, isActive);
            }
            else
            {
                carPhysicsManager.IsActive = isActive;
            }
        }

        /// <summary>
        /// Helper function for SetIsActive() which is called on the server only, which sends
        /// a signal to the client to execute SetIsActive() and change the active status of the
        /// car physics. The server calls this function in SetIsActive(), 
        /// but the body is executed on the client, so there is no infinite loop.
        /// </summary>
        /// <param name="target">The connection to the client, obtained from OwnPlayer</param>
        /// <param name="isActive">Whether the car should be active or not.</param>
        [TargetRpc] 
        void TargetSetIsActive(NetworkConnection target, bool isActive) => SetIsActive(isActive);
    }
}
