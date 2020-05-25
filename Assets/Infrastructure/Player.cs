using Mirror;
using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using Racerr.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public sealed class Player : NetworkBehaviour
    {
        #region Player's Car

        [SyncVar] GameObject carGO;
        CarManager car;
        public CarManager Car
        {
            get
            {
                if (carGO != null && (car == null || (car != null && car.gameObject != carGO)))
                {
                    car = carGO.GetComponent<CarManager>();
                }

                return car;
            }
        }

        class SyncListGameObject : SyncList<GameObject> { }
        readonly SyncListGameObject zombieCarGOs = new SyncListGameObject();
        public IReadOnlyList<GameObject> ZombieCarGOs => zombieCarGOs;

        /// <summary>
        /// Spawn the player's car in a given position, given the prefab to spawn and what type of car it is (Racer, Police).
        /// </summary>
        /// <param name="spawnPosition">Position to spawn.</param>
        /// <param name="spawnRotation">Rotation to spawn.</param>
        /// <param name="carPrefab">Car prefab to spawn.</param>
        /// <param name="carType">The type of car being spawned.</param>
        [Server]
        public void CreateCarForPlayer(Vector3 spawnPosition, Quaternion spawnRotation, GameObject carPrefab, CarManager.CarTypeEnum carType)
        {
            // Instantiate and setup car.
            GameObject carGO = Instantiate(carPrefab, spawnPosition, spawnRotation);
            car = carGO.GetComponent<CarManager>();
            car.PlayerGO = gameObject;
            car.CarType = carType;

            // Setup and sync over network.
            NetworkServer.Spawn(carGO, gameObject);
            this.carGO = carGO;
            Health = Car.MaxHealth;

            // Log for Sentry Breadcrumb
            SentrySdk.AddBreadcrumb($"Created new car for player { PlayerName } with carType { carType }.");
        }

        /// <summary>
        /// A command sent by the client when they are ready to spawn a police car.
        /// Client cannot call SpawnManager directly as Mirror can only send Commands through Player,
        /// so this acts as a proxy. We check preconditions to ensure the police car
        /// can only be spawned when they have died as a racer.
        /// </summary>
        [Command]
        public void CmdSpawnPoliceCarOnFinishingGrid()
        {
            if (!IsDeadAsRacer)
            {
                SentrySdk.AddBreadcrumb("Attempt to spawn police car onto track with failed precondition.");
                return;
            }

            SpawnManager.Singleton.SpawnPoliceCarOnFinishingGrid(this);
        }

        /// <summary>
        /// Mark a car as a zombie, meaning it stays on the track
        /// but cannot be driven.
        /// </summary>
        [Server]
        public void MarkPlayerCarAsZombie()
        {
            if (carGO != null)
            {
                car.SetIsActive(false);
                zombieCarGOs.Add(carGO);
                carGO = null;
                car = null;
                SentrySdk.AddBreadcrumb($"Marked { PlayerName }'s car as zombie.");
            }
        }

        /// <summary>
        /// Destroy all cars for the player, including all
        /// zombie cars and the currently racing player car.
        /// </summary>
        [Server]
        public void DestroyAllCarsForPlayer()
        {
            MarkPlayerCarAsZombie();
            foreach (GameObject zombieCarGO in ZombieCarGOs)
            {
                NetworkServer.Destroy(zombieCarGO);
            }
            zombieCarGOs.Clear();
            SentrySdk.AddBreadcrumb($"Destroyed all cars for player { PlayerName }.");
        }

        #endregion

        #region Player Information

        #region Fields

        [SyncVar] string playerName;
        [SyncVar] bool isReady;
        [SyncVar] int health = 0;
        [SyncVar] PositionInfo positionInfo;
        [SyncVar] bool isAI;

        #endregion

        #region Properties

        public string PlayerName
        {
            get => playerName;
            set
            {
                if (isServer)
                {
                    name = "Player - " + value;
                    playerName = value;
                }
                else
                {
                    CmdSynchronisePlayerName(value);
                }
            }
        }

        public bool IsReady
        {
            get => isReady;
            set
            {
                if (isServer)
                {
                    isReady = value;
                }
                else
                {
                    CmdSynchroniseIsReady(value);
                }
            }
        }

        public int Health
        {
            get => health;
            set 
            {
                value = Math.Max(0, value);

                if (isServer)
                {
                    if (value == 0)
                    {
                        MarkPlayerCarAsZombie();
                    }

                    health = value;
                }
                else
                {
                    CmdSynchroniseHealth(value);
                }
            }
        }

        // They are an alive racer or police.
        public bool IsInRace => Health > 0 && !PosInfo.IsFinished && Car != null;

        // Either the racer just died, they are a police or a dead police.
        public bool IsDeadAsRacer => !PosInfo.IsFinished && ZombieCarGOs.Any(zombieCarGO => zombieCarGO.GetComponent<CarManager>().CarType == CarManager.CarTypeEnum.Racer);

        // They are dead police.
        public bool IsDeadCompletely => !PosInfo.IsFinished && ZombieCarGOs.Any(zombieCarGO => zombieCarGO.GetComponent<CarManager>().CarType == CarManager.CarTypeEnum.Police);

        public PositionInfo PosInfo
        {
            get => positionInfo;
            set
            {
                if (isServer)
                {
                    positionInfo = value;
                }
                else
                {
                    throw new InvalidOperationException("Only server can set this property.");
                }
            }
        }

        public bool IsAI
        {
            get => isAI;
            set
            {
                if (isServer)
                {
                    isAI = value;
                }
                else
                {
                    throw new InvalidOperationException("Only server can set this property.");
                }
            }
        }

        #endregion

        #region Commands for synchronising SyncVars

        /* How it works for CLIENT:
         * 1. Update a Property (above) on a Client.
         * 2. The associated Command is executed on the client by property setter, sending message to server.
         * 3. Associated SyncVar is updated on server, updating the SyncVar on all clients.
         * 
         * How it works for SERVER:
         * 1. Update a Property (above) on the Server.
         * 2. Associated SyncVar is updated by property setter on server, updating the SyncVar on all clients.
         */

        [Command]
        void CmdSynchroniseIsReady(bool isReady)
        {
            IsReady = isReady;
        }

        [Command]
        void CmdSynchronisePlayerName(string playerName)
        {
            PlayerName = playerName;
        }

        [Command]
        void CmdSynchroniseHealth(int health)
        {
            Health = health;
        }

        #endregion

        #endregion

        #region Position Info 

        /// <summary>
        /// Holds Player Position Info so we can determine a player's status in the race.
        /// Some properties are synced with the client so it can determine how to transition the UI.
        /// E.g. if the client discovers they've finished, they should show the spectate UI.
        /// Some properties such as the Checkpoints are not synced as they are used solely by the server.
        /// </summary>
        public readonly struct PositionInfo
        {
            /* Server and Client Properties */
            public readonly double startTime;
            public readonly double finishTime;

            public bool IsFinished => !double.IsPositiveInfinity(finishTime);

            /// <summary>
            /// Returns a properly formatted string (in M:SS.FFF format) showing their race length duration.
            /// </summary>
            public string TimeString
            {
                get
                {
                    if (!IsFinished)
                    {
                        return "DNF";
                    }
                    else
                    {
                        double playerRaceLength = finishTime - startTime;
                        return playerRaceLength.ToRaceTimeFormat();
                    }
                }
            }

            /* Server Only Properties */
            public HashSet<GameObject> Checkpoints { get; }

            public PositionInfo(double startTime, double finishTime = double.PositiveInfinity)
            {
                this.Checkpoints = new HashSet<GameObject>();
                this.startTime = startTime;
                this.finishTime = finishTime;
            }

        }

        #endregion
    }
}