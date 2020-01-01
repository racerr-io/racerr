using Mirror;
using NWH.VehiclePhysics;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Server;
using Racerr.UX.Car;
using Racerr.World.Track;
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
        ServerRaceState serverRaceState;

        [Header("Car Properties")]
        [SerializeField] int maxHealth = 100;
        public int MaxHealth => maxHealth;

        [Header("Player Bar Properties")]
        [SerializeField] GameObject playerBarPrefab;
        [SerializeField] float playerBarStartDisplacement = 4; // Displacement from car centre at all times
        [SerializeField] float playerBarUpDisplacement = 1; // Additional displacement when car is moving south of the screen (need this due to camera angle changes)
        public PlayerBar PlayerBar { get; private set; }
        public float PlayerBarStartDisplacement => playerBarStartDisplacement;
        public float PlayerBarUpDisplacement => playerBarUpDisplacement;

        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set => playerGO = value;
        }

        public Player OwnPlayer { get; private set; }

        CarPhysicsManager carPhysicsManager;
        public float SpeedKPH => carPhysicsManager.SpeedKPH;
        public List<Wheel> Wheels => carPhysicsManager.Wheels;

        /// <summary>
        /// Called when the car is instantiated. Caches various fields for later use
        /// and instantiates the Player's Bar, which should appear above the car in the game.
        /// </summary>
        void Start()
        {
            serverRaceState = FindObjectOfType<ServerRaceState>();
            OwnPlayer = PlayerGO.GetComponent<Player>();
            carPhysicsManager = GetComponent<CarPhysicsManager>();

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.CarManager = this;
        }

        /// <summary>
        /// Detect if the car is moving through triggers, which are GameObjects in the world which result in no collision
        /// and do not affect the motion of the car. We use this to send a message to the Server Race State when a player passes
        /// through a checkpoint, which is an invisible box collider located at the end of every track piece.
        /// </summary>
        /// <param name="collider">The collider that it went through.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == TrackPieceComponent.FinishLineCheckpoint || collider.name == TrackPieceComponent.Checkpoint)
            {
                serverRaceState.NotifyPlayerPassedThroughCheckpoint(OwnPlayer, collider.gameObject);
            }
        }

        /// <summary>
        /// Apply damage to car on collision with other players and the environment (e.g. buildings), by decreasing the players
        /// health by a flat amount.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Environment"))
            {
                OwnPlayer.Health -= 10;
            }
        }

        /// <summary>
        /// Cars initially spawn in an inactive state, so they cannot be driven before the race starts.
        /// Once the race starts, this function is called by the server to allow all players in the race
        /// to drive their car.
        /// </summary>
        /// <param name="isActive">Whether the car should be active or not.</param>
        public void SetIsActive(bool isActive)
        {
            carPhysicsManager.IsActive = isActive;
        }
    }
}
