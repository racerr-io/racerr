using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using System.Net;
using UnityEngine;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Examples
{
    public class Example4_ProgrammaticRequestHandler : MonoBehaviour
    {
        [SerializeField]
        private SimpleProgrammaticEventServerScript _server; 

        void OnEnable()
        {
            _server.OnReceiveRequestEvent -= GetProgrammaticResponse; // Just to make sure we don't register the method twice when reloading!
            _server.OnReceiveRequestEvent += GetProgrammaticResponse;
        }

        [SimpleEventServerRouting(HttpConstants.MethodGet, "/programmatic")]
        public void GetProgrammaticResponse(HttpListenerContext context)
        {
            context.Response.HtmlResponse("<h1>Programmatic Request handling</h1>");
        }
    }
}
