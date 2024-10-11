using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Trustly.Api.Domain.Exceptions;
using Microsoft.Extensions.Primitives;

namespace Trustly.Api.Client
{
    public static class TrustlyApiClientExtensions
    {
        public static readonly string GENERIC_ERROR_MESSAGE = "An exception occurred (error message given by trustly-api-client for .NET)";

        private static readonly AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
        private static readonly Version assemblyVersion = assemblyName.Version;

        public static void UseTrustlyNotifications(this IApplicationBuilder app, TrustlyApiClient client)
        {
            app.Use((context, next) =>
            {
                return HandleNotificationRequest(context, next, client);
            });
        }

        public async static Task HandleNotificationRequest(HttpContext context, Func<Task> next, TrustlyApiClient client)
        {
            var request = context.Request;
            var contextPath = request.Path.Value.Trim(new[] { '/' });

            if (string.Equals(contextPath, client.Settings.NotificationUrl ?? "trustly/notifications", StringComparison.InvariantCultureIgnoreCase))
            {
                var responseCount = 0;
                var includeErrorMessage = false;

                try
                {
                    includeErrorMessage = includeErrorMessage || client.Settings.IncludeExceptionMessageInNotificationResponse;

                    await client.HandleNotificationFromRequestAsync(
                        request,
                        async (stringBody) =>
                        {
                            responseCount++;

                            context.Response.Headers.Add("User-Agent", new StringValues("trustly-api-client/" + assemblyVersion));
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            await context.Response.WriteAsync(stringBody);
                        }
                    );
                }
                catch (Exception ex)
                {
                    context.Response.Headers.Add("User-Agent", new StringValues("trustly-api-client/" + assemblyVersion));
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    if (includeErrorMessage)
                    {
                        await context.Response.WriteAsync(ex.Message);
                        responseCount++;
                    }
                    else
                    {
                        await context.Response.WriteAsync(GENERIC_ERROR_MESSAGE);
                        responseCount++;
                    }
                }

                if (responseCount == 0)
                {
                    throw new TrustlyNoNotificationListenerException("None of your clients' event listeners responded with acknowledge data. That must be done.");
                }
            }
            else
            {
                if (next != null)
                {
                    await next.Invoke();
                }
            }
        }
    }
}
