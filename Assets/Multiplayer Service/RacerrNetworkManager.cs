using Mirror;
using Racerr.RaceSessionManager;
using Racerr.Track;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class RacerrNetworkManager : NetworkManager
    {
        [SerializeField] GameObject playerObject;
        [SerializeField] [Range(1,20)] int connectionWaitTime = 5;
        int SecondsWaitingForConnection { get; set; } = 0;

        /// <summary>
        /// Automatically start the server on headless mode,
        /// or automatically connect client on server on normal mode.
        /// </summary>
        void Start()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // headless mode. Just start the server
                StartServer();
            }
            else
            {
#if UNITY_EDITOR
                StartHost();
#else
                StartClient();
#endif
                StartCoroutine(UpdateStartMenu());
            }
        }

        /// <summary>
        /// Show connection message and update start menu user interface depending on user's connection to server.
        /// </summary>
        /// <returns>IEnumerator for coroutine to run concurrently.</returns>
        IEnumerator UpdateStartMenu()
        {
            while (true)
            {
                StartMenu startMenu = FindObjectOfType<StartMenu>();

                if (IsClientConnected())
                {
                    startMenu.ShowMenu();
                    break;
                }
                else if (SecondsWaitingForConnection >= connectionWaitTime)
                {
                    startMenu.ShowErrorMessage();
                    break;
                }

                SecondsWaitingForConnection++;
                yield return new WaitForSeconds(1);
            }
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            GameObject player = Instantiate(playerObject);
            NetworkServer.AddPlayerForConnection(conn, player);
            RacerrRaceSessionManager.Singleton.AddNewPlayer(player);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
        {
            RacerrRaceSessionManager.Singleton.RemovePlayer(player.gameObject);
            base.OnServerRemovePlayer(conn, player);
        }
    }
}