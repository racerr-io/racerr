using Mirror;
using Racerr.Track;
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

        /// <summary>
        /// Actions to perform when a new player joins the server.
        /// </summary>
        /// <param name="conn">The new player's connection information.</param>
        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            TrackGeneratorCommon.Singleton.GenerateIfRequired();
        }

        /// <summary>
        /// Actions to perform when an existing player disconnects from the server.
        /// </summary>
        /// <param name="conn">The disconnected player's connection information.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            TrackGeneratorCommon.Singleton.DestroyIfRequired();
        }
    }
}