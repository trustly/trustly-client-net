using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;

namespace Trustly.Api.Client
{
    public static class TrustlyApiClientExtensions
    {
        public static void UseTrustlyNotifications(this IApplicationBuilder app)
        {
            app.Use((context, next) => HandleNotificationRequest(context, next));
        }

        public async static Task HandleNotificationRequest(HttpContext context, Func<Task> next)
        {
            var request = context.Request;
            var contextPath = request.Path.Value.Trim(new[] { '/' });

            if (string.Equals(contextPath, "trustly/notifications", StringComparison.InvariantCultureIgnoreCase))
            {
                var responseCount = 0;
                var clientCount = 0;
                foreach (var client in TrustlyApiClient.GetRegisteredClients())
                {
                    clientCount++;
                    client.HandleNotificationFromRequest(
                        request,
                        onOK: (rpcMethod, uuid) =>
                        {
                            responseCount++;
                            Respond(client, context.Response, rpcMethod, uuid, "OK", null, HttpStatusCode.OK);
                        },
                        onFailed: (rpcMethod, uuid, message) =>
                        {
                            responseCount++;
                            Respond(client, context.Response, rpcMethod, uuid, "FAILED", message, HttpStatusCode.InternalServerError);
                        }
                    );
                }

                if (clientCount == 0)
                {
                    throw new TrustlyNoNotificationClientException("There are no registered Api Clients listening to notifications");
                }

                if (responseCount == 0)
                {
                    throw new TrustlyNoNotificationClientException("None of your client's event listeners responded with OK or FAILED. That must be done.");
                }
            }
            else
            {
                await next.Invoke();
            }
        }

        public static void Respond(TrustlyApiClient client, HttpResponse response, string method, string uuid, string status, string message, HttpStatusCode httpStatusCode)
        {
            var rpcResponse = client.CreateResponsePackage(
                method,
                uuid,
                new NotificationResponse
                {
                    Status = status,
                    ExtensionData = new Dictionary<string, object>
                    {
                        { "message", message }
                    }
                }
            );

            var rpcString = JsonConvert.SerializeObject(rpcResponse);
            var rpcBytes = Encoding.UTF8.GetBytes(rpcString);
            var rpcStream = new MemoryStream(rpcBytes);

            response.Body = rpcStream;
            response.StatusCode = (int)httpStatusCode;
        }
    }
}
