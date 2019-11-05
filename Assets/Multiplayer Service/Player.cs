using Mirror;
using Racerr.Car.Core;
using Racerr.UX.Camera;
using System;
using System.Linq;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        #region Local Player

        static Player localPlayer;
        public static Player LocalPlayer => localPlayer ?? (localPlayer = FindObjectsOfType<Player>().SingleOrDefault(p => p.isLocalPlayer));

        #endregion

        #region Player's Car

        [SyncVar] GameObject carGO;
        PlayerCarController car;
        public PlayerCarController Car
        {
            get
            {
                if (car == null && carGO != null)
                {
                    car = carGO.GetComponent<PlayerCarController>();
                }

                return car;
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
            car = carGO.GetComponent<PlayerCarController>();
            car.PlayerGO = gameObject;

            // Setup and sync over network
            NetworkServer.SpawnWithClientAuthority(carGO, gameObject);
            this.carGO = carGO;
            Health = Car.MaxHealth;
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
            car = null;
        }

        #endregion

        #region Player Information

        #region Fields

        [SyncVar] string playerName;
        [SyncVar] bool isReady;
        [SyncVar] [SerializeField] GameObject carPrefab;
        [SyncVar(hook = nameof(OnPlayerHealthChanged))] int health = 100;
        [SyncVar] PlayerPositionInfo positionInfo;

        /// <summary>
        /// When playerHealth SyncVar updates, this function is called to update
        /// the PlayerBar UI.
        /// </summary>
        /// <param name="health">The new health value.</param>
        void OnPlayerHealthChanged(int health)
        {
            this.health = health;
            Car?.PlayerBar?.SetHealthBar(health);
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

                    if (health == 0 && hasAuthority)
                    {
                        FindObjectOfType<TargetObserverCamera>().SetTarget(null);
                    }
                }
            }
        }

        public bool IsDead => Health == 0;

        public PlayerPositionInfo PositionInfo
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
        void CmdSynchronisePositionInfo(PlayerPositionInfo positionInfo)
        {
            this.positionInfo = positionInfo;
        }

        #endregion

        #endregion
    }
}