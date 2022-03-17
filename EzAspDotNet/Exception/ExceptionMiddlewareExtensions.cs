using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System.Net;

namespace EzAspDotNet.Exception
{
    public static class ExceptionMiddlewareExtensions
    {

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        Log.Logger.Error($"Something went wrong: {contextFeature.Error}");
                        if (contextFeature.Error.GetType() == typeof(DeveloperException))
                        {
                            var developerException = (DeveloperException)contextFeature.Error;
                            context.Response.StatusCode = (int)developerException.HttpStatusCode;

                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorDetails
                            {
                                Detail = developerException.Detail,
                                ResultCode = developerException.ResultCode
                            }));
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorDetails
                            {
                                Detail = contextFeature.Error.Message,
                                ResultCode = Protocols.Code.ResultCode.InternalServerError
                            }));
                        }
                    }
                });
            });
        }
    }
}
