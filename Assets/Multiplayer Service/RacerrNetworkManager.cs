using Mirror;
using Racerr.StateMachine.Server;
using Racerr.UX.Camera;
using Racerr.UX.Menu;
using System.Collections;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class RacerrNetworkManager : NetworkManager
    {
        [SerializeField] int serverApplicationFrameRate = 60;
        [SerializeField] GameObject playerObject;

        /// <summary>
        /// Automatically start the server on headless mode (detected through whether it has a graphics device),
        /// or automatically connect client on server on normal mode.
        /// </summary>
        public override void Start()
        {
            if (isHeadless)
            {
                Application.targetFrameRate = serverApplicationFrameRate;

                foreach (AbstractTargetFollower camera in FindObjectsOfType<AbstractTargetFollower>())
                {
                    Destroy(camera.gameObject);
                }

                StartServer();
            }
            else
            {
#if UNITY_EDITOR
                StartHost();
#else
                StartClient();
#endif
            }
        }

        /// <summary>
        /// Upon player joining, add the new player, associate them with the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
        {
            GameObject player = Instantiate(playerObject);
            NetworkServer.AddPlayerForConnection(conn, player);
            ServerStateMachine.Singleton.AddNewPlayer(player);
        }

        /// <summary>
        /// Upon player disconnect, delete the player, remove the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            ServerStateMachine.Singleton.RemovePlayer(conn.playerController.gameObject);
            NetworkServer.DestroyPlayerForConnection(conn);
        }
    }
}