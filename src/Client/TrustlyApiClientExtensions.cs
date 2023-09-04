using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
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
                    await client.HandleNotificationFromRequestAsync(
                        request,
                        onOK: async (rpcMethod, uuid) =>
                        {
                            responseCount++;
                            await Respond(client, context.Response, rpcMethod, uuid, "OK", null, HttpStatusCode.OK);
                        },
                        onFailed: async (rpcMethod, uuid, message) =>
                        {
                            responseCount++;
                            await Respond(client, context.Response, rpcMethod, uuid, "FAILED", message, HttpStatusCode.InternalServerError);
                        },
                        onCustomStatus: async (rpcMethod, uuid, customStatus, message) =>
                        {
                            responseCount++;
                            await Respond(client, context.Response, rpcMethod, uuid, customStatus, message, HttpStatusCode.OK);
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

        public static async Task Respond(TrustlyApiClient client, HttpResponse response, string method, string uuid, string status, string message, HttpStatusCode httpStatusCode)
        {
            var rpcResponse = client.CreateResponsePackage(
                method,
                uuid,
                new NotificationResponse
                {
                    Status = status
                }
            );

            if (client.Settings.IncludeMessageInNotificationResponse)
            {
                if (string.IsNullOrEmpty(message) == false)
                {
                    rpcResponse.Result.Data.ExtensionData = new Dictionary<string, object>
                    {
                        { "message", message }
                    };
                }
            }

            var rpcString = JsonConvert.SerializeObject(rpcResponse);

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var assemblyVersion = assemblyName.Version;

            response.Headers.Add("User-Agent", new Microsoft.Extensions.Primitives.StringValues("trustly-api-client/" + assemblyVersion));
            response.StatusCode = (int)httpStatusCode;
            await response.WriteAsync(rpcString);
        }
    }
}
