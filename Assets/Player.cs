using Mirror;
using Racerr.Car.Core;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Player information, such as their stats and their associated car in the game.
    /// </summary>
    public class Player : NetworkBehaviour
    {
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

        [SyncVar] [SerializeField] string playerName;
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
                    CmdUpdatePlayerName(value);
                }
            }
        }

        void FixedUpdate()
        {
            if (Car == null && isServer)
            {
                Debug.Log("Respawning D Car.");
                CreateCarForPlayer(NetworkManager.singleton.playerPrefab);
            }
        }

        [Command]
        void CmdUpdatePlayerName(string playerName)
        {
            this.playerName = playerName;
        }

        /// <summary>
        /// Spawn the player's car in the correct position.
        /// </summary>
        /// <param name="carPrefab">Car prefab</param>
        [Server]
        public void CreateCarForPlayer(GameObject carPrefab)
        {
            GameObject instantiatedCarGO = Instantiate(carPrefab);
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