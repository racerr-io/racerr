using Mirror;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Server;
using Racerr.World.Track;
using Racerr.UX.Car;
using System;
using UnityEngine;
using NWH.VehiclePhysics;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Car controller for all cars in Racerr.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class CarController : NetworkBehaviour
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

        VehicleController vehicleController;
        Rigidbody vehicleRigidbody;
        public int Velocity => Convert.ToInt32(vehicleRigidbody.velocity.magnitude * 2);

        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set => playerGO = value;
        }

        public Player Player { get; private set; }

        /// <summary>
        /// Called when car instantiated. Setup the user's view of the car.
        /// </summary>
        void Start()
        {
            serverRaceState = FindObjectOfType<ServerRaceState>();

            Player = PlayerGO.GetComponent<Player>();
            vehicleRigidbody = GetComponent<Rigidbody>();
            vehicleController = GetComponent<VehicleController>();

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.Car = this;
        }

        /// <summary>
        /// Detect if the car is moving through triggers.
        /// </summary>
        /// <param name="collider">The collider that it went through.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.name == TrackPieceComponent.FinishLineCheckpoint || collider.name == TrackPieceComponent.Checkpoint)
            {
                serverRaceState.NotifyPlayerPassedThroughCheckpoint(Player, collider.gameObject);
            }
        }

        /// <summary>
        /// Apply damage to car on collision with other players and world objects.
        /// </summary>
        /// <param name="collision">Collision information</param>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Environment"))
            {
                Player.Health -= 10;
            }
        }

        [ClientRpc]
        public void RpcSetCarActiveState(bool active)
        {
            vehicleController.Active = active;
        }
    }
}
