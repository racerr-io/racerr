using Mirror;
using Racerr.Car.Core;
using Racerr.RaceSessionManager;
using System.Linq;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        static Player localPlayer;
        public static Player LocalPlayer => localPlayer ?? (localPlayer = FindObjectsOfType<Player>().Single(p => p.isLocalPlayer));

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

        /// <summary>
        /// Spawn the player's car in the correct position.
        /// </summary>
        /// <param name="carPrefab">Car prefab</param>
        [Server]
        public void CreateCarForPlayer(GameObject carPrefab)
        {
            CreateCarForPlayer(carPrefab, carPrefab.transform.position);
        }

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
    }

}