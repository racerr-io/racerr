using Mirror;
using NWH.VehiclePhysics;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Client;
using Racerr.Utility;
using Racerr.UX.Car;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Car Manager for all cars in Racerr.
    /// Adds Racerr specific customisation to the vehicle, such as health and the player bar.
    /// WARNING: Car must be automatically generated using Instantiate() and certain fields set (see contracts in Start()).
    /// </summary>
    [RequireComponent(typeof(CarPhysicsManager))]
    public class CarManager : NetworkBehaviour
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] GameObject playerBarPrefab;

        /* Player */
        public Player OwnPlayer { get; private set; }
        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set
            {
                Contract.Assert(playerGO == null, "PlayerGO must only be set once on instantiation.");
                playerGO = value;
            }
        }

        /* Player Bar */
        public PlayerBar PlayerBar { get; private set; }

        /* Health */
        public int MaxHealth => maxHealth;
        const double otherCarDamageAdjustmentFactor = 0.00004f;
        const double environmentDamageAdjustmentFactor = 0.00002f;
        [SyncVar] GameObject lastHitByPlayerGO;
        public Player LastHitByPlayer
        {
            get
            {
                if (lastHitByPlayerGO != null)
                {
                    return lastHitByPlayerGO.GetComponentInParent<Player>();
                }

                return null;
            }
        }

        /* Car Type */
        public enum CarTypeEnum
        {
            Unset,
            Racer,
            Police
        }
        [SyncVar] CarTypeEnum carType = CarTypeEnum.Unset;
        public CarTypeEnum CarType
        {
            get => carType;
            set
            {
                Contract.Assert(carType == CarTypeEnum.Unset, "CarType must only be set once on instantiation.");
                Contract.Assert(value != CarTypeEnum.Unset, "CarType cannot be set to Unset.");
                carType = value;
            }
        }

        /* Physics */
        CarPhysicsManager carPhysicsManager;
        public float SpeedKPH => carPhysicsManager.SpeedKPH;
        public List<Wheel> Wheels => carPhysicsManager.Wheels;
        
        /// <summary>
        /// Called when the car is instantiated. Caches various fields for later use
        /// and instantiates the Player's Bar, which should appear above the car in the game.
        /// Assumes PlayerGO and CarType has been set immediately after the car was Instantiate()'d.
        /// </summary>
        void Start()
        {
            Contract.Assert(PlayerGO != null, "PlayerGO must be set after instantiating the object.");
            Contract.Assert(carType != CarTypeEnum.Unset, "CarType must be set after instantiating the object.");

            OwnPlayer = PlayerGO.GetComponent<Player>();
            carPhysicsManager = GetComponent<CarPhysicsManager>();

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.CarManager = this;
        }

        /// <summary>
        /// Damage our self when hit by another player's front of car, the environment or the back of the other car 
        /// hits the back of our car, by decreasing the player's health by an amount proportional to the force of the collision. 
        /// The purpose of this is to minimise the chance of the aggressor car taking damage when ramming into other cars.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        [ClientCallback]
        void OnCollisionEnter(Collision collision)
        {
            if (!hasAuthority || OwnPlayer.Health == 0 || OwnPlayer.CarManager != this)
            {
                return;
            }

            ContactPoint contactPoint = collision.GetContact(0);
            bool isHitByEnvironment = collision.gameObject.CompareTag(GameObjectIdentifiers.Environment);
            bool isHitByOtherCarFront = contactPoint.otherCollider.gameObject.CompareTag(GameObjectIdentifiers.CarFrontCollider);
            bool isHitByOtherCarBackIntoOurBack = contactPoint.thisCollider.gameObject.CompareTag(GameObjectIdentifiers.CarBackCollider) && contactPoint.otherCollider.gameObject.CompareTag(GameObjectIdentifiers.CarBackCollider);

            Vector3 collisionForce = collision.impulse / Time.fixedDeltaTime;
            if (isHitByEnvironment)
            {
                OwnPlayer.Health -= Convert.ToInt32(collisionForce.magnitude * environmentDamageAdjustmentFactor);
            }
            else if (isHitByOtherCarFront || isHitByOtherCarBackIntoOurBack)
            {
                CmdSetLastHitPlayerGO(contactPoint.otherCollider.gameObject.GetComponentInParent<CarManager>().OwnPlayer.gameObject);
                OwnPlayer.Health -= Convert.ToInt32(collisionForce.magnitude * otherCarDamageAdjustmentFactor);
            }
        }

        /// <summary>
        /// Command send by the client to update the LastHitByPlayer on the server.
        /// </summary>
        /// <param name="lastHitByPlayerGO">The player GameObject of the player who hit this car.</param>
        [Command]
        void CmdSetLastHitPlayerGO(GameObject lastHitByPlayerGO)
        {
            this.lastHitByPlayerGO = lastHitByPlayerGO;
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
