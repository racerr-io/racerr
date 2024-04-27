using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Control;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ShadowGroveGames.SimpleHttpAndRestServer.Scripts
{
    public class SimpleEventServerScript : AbstractServerScript
    {
        [Header("Routing Options")]
        public bool CaseSensitiveRouting = true;

        [Header("Events")]
        public OnReceiveRequestEvent OnReceiveRequest = new OnReceiveRequestEvent();

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

            int eventCount = OnReceiveRequest?.GetPersistentEventCount() ?? 0;
            if (eventCount == 0)
            {
                if (ProcessOptionsRequest(context))
                    return;

                base.ProcessReceivedRequest(context);
                return;
            }

            var matchedRoutes = new HashSet<string>();

            for (int index = 0; index < eventCount; index++)
            {
                #region Validate target
                var targetObject = OnReceiveRequest.GetPersistentTarget(index);
                if (targetObject == null)
                    continue;

                string targetMethodName = OnReceiveRequest.GetPersistentMethodName(index);
                if (targetMethodName == null)
                    continue;

                var targetObjectType = targetObject.GetType();
                if (targetObjectType == null)
                    continue;

                var targetObjectMethodType = targetObjectType.GetMethod(targetMethodName);
                if (targetObjectMethodType == null)
                    continue;
                #endregion

                #region Get and validate SimpleEventServerRoutingAttribute
                SimpleEventServerRoutingAttribute simpleRouting = targetObjectMethodType.GetCustomAttributes(true).OfType<SimpleEventServerRoutingAttribute>().FirstOrDefault();
                if (simpleRouting == null)
                {
                    Debug.LogError($"Method \"{targetMethodName}\" in Listener \"{targetObjectType.FullName}\" missing SimpleEventServerRouting attribute!");
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
                targetObjectMethodType.Invoke(targetObject, new object[1] { context });
            }

            if (ProcessOptionsRequest(context))
                return;

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

    [Serializable]
    public class OnReceiveRequestEvent : SimpleEventServerEndpoints<HttpListenerContext> { }
}