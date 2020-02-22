using Mirror;
using Racerr.Gameplay.Car;
using Racerr.Utility;
using Racerr.World.Track;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public sealed class Player : NetworkBehaviour
    {
        #region Player's Car

        /* Client and Server Properties */
        [SerializeField] GameObject raceCarPrefab;
        [SerializeField] GameObject policeCarPrefab;

        [SyncVar] GameObject carGO;
        CarManager carManager;
        public CarManager CarManager
        {
            get
            {
                if (carGO != null && (carManager == null || (carManager != null && carManager.gameObject != carGO)))
                {
                    carManager = carGO.GetComponent<CarManager>();
                }

                return carManager;
            }
        }

        /* Server Only Properties */
        List<GameObject> ZombieCarGOs { get; } = new List<GameObject>();

        /// <summary>
        /// Spawn race car for the player. Intended to be called by the Track Generator.
        /// </summary>
        /// <param name="spawnPosition">Position to spawn.</param>
        [Server]
        public void CreateRaceCarForPlayer(Vector3 spawnPosition)
        {
            CreateCarForPlayer(spawnPosition, raceCarPrefab, CarManager.CarTypeEnum.Racer);
        }

        /// <summary>
        /// A command sent by the client when they are ready to spawn a police car.
        /// This is specifically sent by the client only when they have died as a racer and are ready
        /// to spawn the police car. This cannot be called at any other time.
        /// We will spawn the police car at the finish line, so they can get revenge on the racers.
        /// <remarks>
        /// We assume that by default the police car prefab's vehicle controller is set to active, so
        /// the car is immediately driveable after it is spawned (unlike the other cars, which are by default
        /// set to inactive and are activated when transitioning into Server Race State).
        /// </remarks>
        /// </summary>
        [Command]
        public void CmdCreatePoliceCarForPlayer()
        {
            if (CarManager.CarType == CarManager.CarTypeEnum.Racer && IsDeadAsRacer)
            {
                // Grab finish line and add slight offset to finish line so car is not spawned inside the ground.
                // Need smarter way of spawn police cars, could spawn multiple cars onto the same spot...
                GameObject[] checkpointsInRace = TrackGenerator.Singleton.CheckpointsInRace;
                Vector3 finishLine = checkpointsInRace[checkpointsInRace.Length - 1].transform.position;
                Vector3 spawnPosition = finishLine + new Vector3(0, 0.2f, 0);

                // Spawn.
                CreateCarForPlayer(spawnPosition, policeCarPrefab, CarManager.CarTypeEnum.Police);
            }
        }

        /// <summary>
        /// Spawn the player's car in a given position, given the prefab to spawn and what type of car it is (Racer, Police).
        /// </summary>
        /// <param name="spawnPosition">Position to spawn.</param>
        /// <param name="carPrefab">Car prefab to spawn.</param>
        /// <param name="carType">The type of car being spawned.</param>
        [Server]
        void CreateCarForPlayer(Vector3 spawnPosition, GameObject carPrefab, CarManager.CarTypeEnum carType)
        {
            // Mark any existing car as zombie (their car just stays on the track, chilling).
            MarkPlayerCarAsZombie();

            // Instantiate and setup car.
            GameObject carGO = Instantiate(carPrefab, spawnPosition, carPrefab.transform.rotation);
            carManager = carGO.GetComponent<CarManager>();
            carManager.PlayerGO = gameObject;
            carManager.CarType = carType;

            // Setup and sync over network.
            NetworkServer.Spawn(carGO, gameObject);
            this.carGO = carGO;
            Health = CarManager.MaxHealth;
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
                carManager.SetIsActive(false);
                ZombieCarGOs.Add(carGO);
                carGO = null;
                carManager = null;
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
            ZombieCarGOs.ForEach(NetworkServer.Destroy);
            ZombieCarGOs.Clear();
        }

        #endregion

        #region Player Information

        #region Fields

        [SyncVar] string playerName;
        [SyncVar] bool isReady;
        [SyncVar(hook = nameof(OnPlayerHealthChanged))] int health = 0;
        [SyncVar] PositionInfo positionInfo;

        /// <summary>
        /// When playerHealth SyncVar updates, this function is called to update
        /// the PlayerBar UI and deactive the car if it has ran out of health.
        /// </summary>
        /// <param name="health">The new health value.</param>
        void OnPlayerHealthChanged(int health)
        {
            this.health = health;
            
            if (CarManager != null && CarManager.PlayerBar != null)
            {
                CarManager.PlayerBar.SetHealthBar(health);
            }

            if (CarManager != null && health == 0)
            {
                CarManager.SetIsActive(false);
            }
        }

        #endregion

        #region Properties

        public string PlayerName
        {
            get => playerName;
            set
            {
                if (isServer)
                {
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
                    health = value;
                }
                else
                {
                    OnPlayerHealthChanged(value);
                    CmdSynchroniseHealth(value);
                }
            }
        }

        // They are an alive racer or police.
        public bool IsInRace => Health > 0 && !PosInfo.IsFinished && CarManager != null;

        // Either the racer just died, they are a police or a dead police.
        public bool IsDeadAsRacer => !PosInfo.IsFinished && (Health == 0 || CarManager == null || CarManager.CarType == CarManager.CarTypeEnum.Police);

        // They are dead police.
        public bool IsDeadCompletely => !PosInfo.IsFinished && Health == 0 && (CarManager == null || CarManager.CarType == CarManager.CarTypeEnum.Police);

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
                    CmdSynchronisePositionInfo(value);
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
            this.isReady = isReady;
        }

        [Command]
        void CmdSynchronisePlayerName(string playerName)
        {
            this.playerName = playerName;
        }

        [Command]
        void CmdSynchroniseHealth(int health)
        {
            this.health = health;
        }

        [Command]
        void CmdSynchronisePositionInfo(PositionInfo positionInfo)
        {
            this.positionInfo = positionInfo;
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