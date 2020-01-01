using Mirror;
using UnityEngine;
using Racerr.Infrastructure.Server;
using System;

namespace Racerr.Infrastructure
{
    /// <summary>
    /// Customised version of the Network Manager for our needs.
    /// </summary>
    public class ServerManager : NetworkManager
    {
#if UNITY_EDITOR
        enum UnityEditorDebugModeEnum
        {
            Host,
            ClientLocal,
            ClientOnline,
            Headless
        }

        [Header("Debug Mode")]
        [SerializeField] UnityEditorDebugModeEnum unityEditorDebugMode;
#endif
        [Header("Other")]
        [SerializeField] GameObject[] destroyOnServerLoad;

        /// <summary>
        /// Automatically start the server on headless mode (detected through whether it has a graphics device),
        /// or automatically connect client on server on normal mode.
        /// </summary>
        public override void Start()
        {
#if UNITY_EDITOR
            InitialiseNetworking();
#else
            if (isHeadless)
            {
                StartHeadless();
            }
            else 
            {
                StartClient();
            }
#endif
        }

        /// <summary>
        /// Host Mode - An option to start both the server and the client in Unity Editor.
        /// Client Local Debug Mode - An option for client to connect to localhost instead of the specified networkAddress, 
        /// so that we can connect several players locally through several Unity Editors.
        /// Client Online Debug Mode - An option for client to connect to networkAddress and act as a client, 
        /// so we can play on racerr.io through the Unity Editor.
        /// Headless Debug Mode - An option to start the Unity editor in headless mode(the mode our server runs in on AWS), 
        /// so we can debug the headless functionality.
        /// </summary>
        void InitialiseNetworking()
        {
            switch (unityEditorDebugMode)
            {
                case UnityEditorDebugModeEnum.Host: StartHost(); break;
                case UnityEditorDebugModeEnum.ClientOnline: StartClient(); break;
                case UnityEditorDebugModeEnum.ClientLocal: networkAddress = "localhost"; StartClient(); break;
                case UnityEditorDebugModeEnum.Headless: StartHeadless(); break;
                default: throw new InvalidOperationException("Invalid Unity Editor Debug Mode attempt: " + unityEditorDebugMode.ToString());
            }
        }

        void StartHeadless()
        {
            Application.targetFrameRate = serverTickRate;
            DestroySelectedGameObjectsOnServerLoad();
            StartServer();
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