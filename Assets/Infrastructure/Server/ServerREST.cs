#if !UNITY_WEBGL
using Newtonsoft.Json.Linq;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;
using System.Net;
#endif
using UnityEngine;

public class ServerREST : MonoBehaviour
{
    #if !UNITY_WEBGL
    [SimpleEventServerRouting(HttpConstants.MethodGet, "/healthcheck")]
    public void Healthcheck(HttpListenerContext context)
    {
        context.Response.JsonResponse(new JObject() {
            new JProperty("healthy", true),
        });
    }
    #endif
}