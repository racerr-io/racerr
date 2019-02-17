﻿using Mirror;
using Racerr.RaceSessionManager;
using Racerr.UX.Camera;
using Racerr.UX.HUD;
using Racerr.UX.Menu;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class RacerrNetworkManager : NetworkManager
    {
        [SerializeField] int targetApplicationFrameRate = 60;
        [SerializeField] GameObject playerObject;
        [SerializeField] [Range(1,20)] int connectionWaitTime = 5;
        int secondsWaitingForConnection = 0;

        /// <summary>
        /// Automatically start the server on headless mode,
        /// or automatically connect client on server on normal mode.
        /// </summary>
        void Start()
        {
            Application.targetFrameRate = targetApplicationFrameRate;

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // headless mode. Just start the server
                StartServer();
                Destroy(FindObjectOfType<StartMenu>().gameObject);
                Destroy(FindObjectOfType<AutoCam>().gameObject);
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
                else if (secondsWaitingForConnection >= connectionWaitTime)
                {
                    startMenu.ShowErrorMessage();
                    break;
                }

                secondsWaitingForConnection++;
                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Upon player joining, add the new player, associate them with the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            GameObject player = Instantiate(playerObject);
            NetworkServer.AddPlayerForConnection(conn, player);
            RacerrRaceSessionManager.Singleton.AddNewPlayer(player);
        }

        /// <summary>
        /// Upon player disconnect, delete the player, remove the Player game object and synchronise on all clients.
        /// </summary>
        /// <param name="conn">Player's connection info.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            RacerrRaceSessionManager.Singleton.RemovePlayer(conn.playerController.gameObject);
            NetworkServer.DestroyPlayerForConnection(conn);
        }
    }
}