using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Trustly.Api.Domain.Exceptions;

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
                try
                {
                    var handledCount = 0;
                    foreach (var client in TrustlyApiClient.GetRegisteredClients())
                    {
                        client.HandleNotificationFromRequest(request);
                        handledCount++;
                    }

                    if (handledCount == 0)
                    {
                        throw new TrustlyNoNotificationClientException("There are no registered Api Clients listening to notifications");
                    }

                    // Send back OK in response, since no exception was thrown
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    try
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    catch (Exception)
                    {
                        // Ignore any error thrown here.
                    }

                    throw;
                }
            }
            else
            {
                await next.Invoke();
            }
        }
    }
}
