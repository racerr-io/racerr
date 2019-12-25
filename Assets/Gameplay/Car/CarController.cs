﻿using Mirror;
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
    /// The class extends off NWH Vehicle Physics, which takes care of the driving mechanics of the vehicle.
    /// These can be customised in the inspector.
    /// This class adds Racerr specific customisation to the vehicle, such as health and the player bar.
    /// </summary>
    public class CarController : VehicleController
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

        public int Velocity => Convert.ToInt32(vehicleRigidbody.velocity.magnitude * 2);

        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set => playerGO = value;
        }

        public Player Player { get; private set; }

        /// <summary>
        /// Called when the car is instantiated. Caches various fields for later use
        /// and instantiates the Player's Bar, which should appear above the car in the game.
        /// </summary>
        void Start()
        {
            serverRaceState = FindObjectOfType<ServerRaceState>();
            Player = PlayerGO.GetComponent<Player>();

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.Car = this;
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
                serverRaceState.NotifyPlayerPassedThroughCheckpoint(Player, collider.gameObject);
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
                Player.Health -= 10;
            }
        }
    }
}
