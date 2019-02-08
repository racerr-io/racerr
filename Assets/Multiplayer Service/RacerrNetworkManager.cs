using Mirror;
using Racerr.Track;
using UnityEngine;
using UnityEngine.Rendering;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class RacerrNetworkManager : NetworkManager
    {
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
                StartClient();
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