using Mirror;
using Racerr.Car.Core;
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
        /// Spawn the player's car in the correct position.
        /// </summary>
        /// <param name="carPrefab">Car prefab</param>
        [Server]
        public void CreateCarForPlayer(GameObject carPrefab)
        {
            CreateCarForPlayer(carPrefab, carPrefab.transform.position);
        }

        /// <summary>
        /// Spawn the player's car in a given position.
        /// </summary>
        /// <param name="carPrefab">Car prefab</param>
        /// <param name="carPosition">Position to spawn</param>
        [Server]
        public void CreateCarForPlayer(GameObject carPrefab, Vector3 carPosition)
        {
            GameObject instantiatedCarGO = Instantiate(carPrefab, carPosition, carPrefab.transform.rotation);
            car = instantiatedCarGO.GetComponent<PlayerCarController>();
            car.PlayerGO = gameObject;
            NetworkServer.SpawnWithClientAuthority(instantiatedCarGO, gameObject);
            carGO = instantiatedCarGO;
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

        #region SyncVars

        [SyncVar] [SerializeField] string playerName;
        [SyncVar] bool isReady;

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

        #endregion

        #endregion
    }
}