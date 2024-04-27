using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using System.Linq;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Examples
{
    public class Example4_OpenInBrowser : MonoBehaviour
    {
        public AbstractServerScript server;

        private int _port = 0;

        private void Start()
        {
            var listeningAddress = server.ListeningAddresses.FirstOrDefault();
            if (listeningAddress == null)
                return;

            _port = listeningAddress.Port;
        }

        public void OpenInBrowser()
        {
            if (_port == 0)
            {
                Debug.LogError("Add a port to the Simple Programmatic Event Server!");
                return;
            }

            Application.OpenURL($"http://127.0.0.1:{_port}/programmatic");
        }
    }
}
