using Newtonsoft.Json;
using System.IO;
using System.Net;
using UnityEngine;

#nullable enable
namespace ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions
{
    public static class HttpListenerRequestExtension
    {
        public static string? GetStringBody(this HttpListenerRequest request)
        {
            if (request.InputStream == null || request.ContentLength64 == 0)
                return null;

            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static T? GetJsonBody<T>(this HttpListenerRequest request) where T : struct
        {
            if (request.InputStream == null || request.ContentLength64 == 0)
                return null;

            if (request.ContentType != HttpConstants.ContentTypeApplicationJson)
            {
                Debug.LogError($"Request missing Content-Type: {HttpConstants.ContentTypeApplicationJson}");
                return null;
            }

            string? rawString = GetStringBody(request);
            if (string.IsNullOrEmpty(rawString))
                return null;

#pragma warning disable CS8604 // We suppress here CS8604 because rawString can't be null after the IsNullOrEmpty check
            return JsonConvert.DeserializeObject<T>(rawString);
#pragma warning restore CS8604
        }
    }
}
#nullable disable
