using Mirror;
using Racerr.Gameplay.Car;
using Racerr.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        #region Player's Car

        [SyncVar] GameObject carGO;
        CarManager carManager;
        public CarManager CarManager
        {
            get
            {
                if (carManager == null && carGO != null)
                {
                    carManager = carGO.GetComponent<CarManager>();
                }

                return carManager;
            }
        }

        /// <summary>
        /// Spawn the player's car in a given position.
        /// </summary>
        /// <param name="carPosition">Position to spawn</param>
        [Server]
        public void CreateCarForPlayer(Vector3 carPosition)
        {
            // Instantiate and setup car
            GameObject carGO = Instantiate(carPrefab, carPosition, carPrefab.transform.rotation);
            carManager = carGO.GetComponent<CarManager>();
            carManager.PlayerGO = gameObject;

            // Setup and sync over network
            NetworkServer.Spawn(carGO, gameObject);
            this.carGO = carGO;
            Health = CarManager.MaxHealth;
        }

        /// <summary>
        /// Destroy the Player's car from the server.
        /// </summary>
        [Server]
        public void DestroyPlayersCar()
        {
            NetworkServer.Destroy(carGO);
            Destroy(carGO);
            carGO = null;
            carManager = null;
        }

        #endregion

        #region Player Information

        #region Fields

        [SyncVar] string playerName;
        [SyncVar] bool isReady;
        [SyncVar] [SerializeField] GameObject carPrefab;
        [SyncVar(hook = nameof(OnPlayerHealthChanged))] int health = 0;
        [SyncVar] PositionInfo positionInfo;

        /// <summary>
        /// When playerHealth SyncVar updates, this function is called to update
        /// the PlayerBar UI.
        /// </summary>
        /// <param name="health">The new health value.</param>
        void OnPlayerHealthChanged(int health)
        {
            this.health = health;
            
            if (CarManager != null && CarManager.PlayerBar != null)
            {
                CarManager.PlayerBar.SetHealthBar(health);
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

        public GameObject CarPrefab
        {
            get => carPrefab;
            set
            {
                if (isServer)
                {
                    carPrefab = value;
                }
                else
                {
                    CmdSynchroniseCarPrefab(value);
                }
            }
        }

        public int Health
        {
            get => health;
            set
            {
                health = Math.Max(0, value);

                if (!isServer)
                {
                    CmdSynchroniseHealth(health);
                }
            }
        }

        public bool IsDead => Health == 0;
        public bool IsRacing => !IsDead && !PosInfo.IsFinished && CarManager != null;

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
        void CmdSynchroniseCarPrefab(GameObject carPrefab)
        {
            this.carPrefab = carPrefab;
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