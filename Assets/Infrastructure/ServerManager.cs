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
        [Header("Other")]
        [SerializeField] GameObject[] destroyOnHeadlessLoad;
#if UNITY_EDITOR
        enum EditorDebugModeEnum
        {
            Host,
            Headless,
            ClientLocal,
            ClientOnline
        }

        [SerializeField] EditorDebugModeEnum editorDebugMode;
#endif

        /// <summary>
        /// On game start, initialise the networking infrastructure.
        /// </summary>
        public override void Start()
        {
#if UNITY_EDITOR
            InitialiseDebugModeNetworking();
#else
            InitialiseNetworking();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Initialises networking in debug mode, intended for use when working with the game in the Unity Editor. You may
        /// select the desired debugging mode via the dropdown in the inspector.
        /// </summary>
        /// <remarks>
        /// Possible debugging modes:
        /// Host - An option to start both the server and the client.
        /// Headless - An option to start the Unity editor in headless server mode (<see cref="StartHeadless"/>).
        /// Client Local - An option for client to connect to localhost instead of the specified networkAddress, 
        /// so that we can connect several players locally through several Unity Editors on the same PC.
        /// Client Online - An option for client to connect to the online server and act as a client, 
        /// so that we can play on racerr.io through the Unity Editor.
        /// </remarks>
        void InitialiseDebugModeNetworking()
        {
            switch (editorDebugMode)
            {
                case EditorDebugModeEnum.Host: StartHost(); break;
                case EditorDebugModeEnum.Headless: StartHeadless(); break;
                case EditorDebugModeEnum.ClientOnline: StartClient(); break;
                case EditorDebugModeEnum.ClientLocal: networkAddress = "localhost"; StartClient(); break;
                default: throw new InvalidOperationException("Invalid Unity Editor Debug Mode attempt: " + editorDebugMode);
            }
        }
#endif

        /// <summary>
        /// Initialises networking for the compiled version of the game. If we have no graphics card (i.e. having no head), 
        /// we will assume we are a headless server (<see cref="StartHeadless"/>), otherwise we are a client.
        /// </summary>
        void InitialiseNetworking()
        {
            if (isHeadless)
            {
                StartHeadless();
            }
            else
            {
                StartClient();
            }
        }

        /// <summary>
        /// Special mode designed especially for deployment to a server. This will destroy all GameObjects
        /// which are only useful for the client (such as the UI) to optimise server performance.
        /// We also limit the frame rate, as there is no point having the frame rate of the server be higher
        /// than the server update frequency.
        /// </summary>
        void StartHeadless()
        {
            foreach (GameObject gameObject in destroyOnHeadlessLoad)
            {
                Destroy(gameObject);
            }

            Application.targetFrameRate = serverTickRate;
            StartServer();
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