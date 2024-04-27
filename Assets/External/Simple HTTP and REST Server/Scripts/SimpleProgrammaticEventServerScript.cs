using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Scripts
{
    public class SimpleProgrammaticEventServerScript : AbstractServerScript
    {
        [Header("Routing Options")]
        [SerializeField]
        public bool CaseSensitiveRouting = true;

        public new event OnReceiveRequest OnReceiveRequestEvent;

        protected override void OnEnable()
        {
            base.OnEnable();

            listener.Start();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected void OnDestroy()
        {
            base.OnDisable();
        }

        protected override void ProcessReceivedRequest(HttpListenerContext context)
        {
            AppendCorsHeader(context);

            var matchedRoutes = new HashSet<string>();

            if (OnReceiveRequestEvent == null)
                base.ProcessReceivedRequest(context);

            foreach (var requestHandler in OnReceiveRequestEvent.GetInvocationList())
            {
                var targetMethod = requestHandler.Method;
                var targetClassType = targetMethod.DeclaringType;

                #region Get and validate SimpleEventServerRoutingAttribute
                SimpleEventServerRoutingAttribute simpleRouting = targetMethod.GetCustomAttributes(true).OfType<SimpleEventServerRoutingAttribute>().FirstOrDefault();
                if (simpleRouting == null)
                {
                    Debug.LogError($"Method \"{targetMethod.Name}\" in Listener \"{targetClassType.FullName}\" missing SimpleEventServerRouting attribute!");
                    base.ProcessReceivedRequest(context);
                    return;
                }
                #endregion

                var incomingHttpMethod = context.Request.HttpMethod.ToUpper();
                var simpleRoutingHttpMethod = simpleRouting.Method.ToUpper();

                if (incomingHttpMethod != simpleRoutingHttpMethod)
                    continue;

                var incomingRoute = context.Request.Url.AbsolutePath.TrimStart('/');
                var simpleRoutingRoute = simpleRouting.Route.Trim('/');

                if (!CaseSensitiveRouting)
                {
                    incomingRoute = incomingRoute.ToLower();
                    simpleRoutingRoute = simpleRoutingRoute.ToLower();
                }

                if (incomingRoute != simpleRoutingRoute)
                    continue;

                string routeKey = $"{incomingHttpMethod}-{incomingRoute}";
                if (matchedRoutes.Contains(routeKey))
                {
                    Debug.LogError($"On the route \"{context.Request.Url.AbsolutePath}\" with the method \"{context.Request.HttpMethod}\" is more than one listener! More than one listener per method/route combination is not allowed.");
                    continue;
                }

                matchedRoutes.Add(routeKey);
                targetMethod.Invoke(requestHandler.Target, new object[1] { context });
            }

            if (matchedRoutes.Count == 0)
                base.ProcessReceivedRequest(context);
        }

        protected virtual bool ProcessOptionsRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod != HttpConstants.MethodOptions)
                return false;

            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            context.Response.Close();

            return true;
        }
    }

    public delegate void OnReceiveRequest(HttpListenerContext context);
}