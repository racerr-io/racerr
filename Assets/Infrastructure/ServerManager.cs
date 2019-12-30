using Mirror;
using UnityEngine;
using Racerr.Infrastructure.Server;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class ServerManager : NetworkManager
    {
        [Header("Other")]
        [SerializeField] GameObject[] destroyOnServerLoad;

#if UNITY_EDITOR
        [SerializeField] bool clientDebugMode;
#endif

        /// <summary>
        /// Automatically start the server on headless mode (detected through whether it has a graphics device),
        /// or automatically connect client on server on normal mode.
        /// </summary>
        public override void Start()
        {
            if (isHeadless)
            {
                Application.targetFrameRate = serverTickRate;
                DestroySelectedGameObjectsOnServerLoad();
                StartServer();
            }
            else
            {
#if UNITY_EDITOR
                if (clientDebugMode)
                {
                    networkAddress = "localhost";
                    StartClient();
                }
                else
                {
                    StartHost();
                }
#else
                StartClient();
#endif
            }
        }

        /// <summary>
        /// Remove game objects which are only specific to the client, to optimise server performance.
        /// </summary>
        void DestroySelectedGameObjectsOnServerLoad()
        {
            foreach (GameObject gameObject in destroyOnServerLoad)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Upon player joining, add the new player, associate them with the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
            ServerStateMachine.Singleton.AddNewPlayer(player);
        }

        /// <summary>
        /// Upon player disconnect, delete the player, remove the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            ServerStateMachine.Singleton.RemovePlayer(conn.identity.gameObject);
            NetworkServer.DestroyPlayerForConnection(conn);
        }
    }
}