using EzAspDotNet.Exception;
using EzAspDotNet.Filter;
using EzAspDotNet.Services;
using EzAspDotNet.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Reflection;

namespace EzAspDotNet.StartUp
{
    public static class Extend
    {
        public static void CommonConfigureServices(this IServiceCollection services)
        {
            if (File.Exists("serilog.json"))
            {
                var seriLogJson = new ConfigurationBuilder()
                                      .AddJsonFile("serilog.json")
                                      .Build();

                Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(seriLogJson)
                                .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Error()
                    .WriteTo.Console()
                    .WriteTo.File($"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}" +
                                  $"/logs/{System.Diagnostics.Process.GetCurrentProcess().ProcessName}_.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }

            services.AddHttpClient();

            services.AddTransient<MongoDbService>();
            services.AddTransient<HttpClientService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.None;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.None;
            });

            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                {
                    // Return JSON responses in LowerCase?
                    options.SerializerSettings.ContractResolver = new CustomJsonResolver();

                    // Resolve Looping navigation properties
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                        c.SchemaFilter<SwaggerExcludeFilter>();
                    });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddMvcCore(options =>
            {
                options.Filters.Add(typeof(ValidateModelFilter));
            });
        }

        public static void CommonConfigure(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureExceptionHandler();

            app.UseCookiePolicy();

            app.UseMvc();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "My API V1");
            });
        }
    }
}
