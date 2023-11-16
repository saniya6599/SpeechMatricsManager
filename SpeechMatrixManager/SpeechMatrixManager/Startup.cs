using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SpeechMatrixManager.Repository.Home;
using SpeechMatrixManager.Repository.PingPongService;
using SpeechMatrixManager.Repository.WebSocketManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SpeechMatrixManager
{
    public class Startup
    {


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SpeechMatrixManager", Version = "v1" });
            });

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader());
            });

            services.AddScoped<IHomeService, HomeService>();
            // Add WebSocketManager as a singleton service
            //services.AddSingleton<WebSocketManager>();


            // Add WebSocketManager as a singleton service
            services.AddSingleton<SocketManager>(provider =>
            {
                // Retrieve the Speechmatics authentication token from your configuration or secrets
                string authToken = Configuration.GetValue<string>("SpeechMatics:AuthToken");
                return new SocketManager(authToken);
            });

            //services.AddSingleton<PingPongMechanism>();
            services.AddSingleton(new PingPongMechanism(new Uri("wss://neu.rt.speechmatics.com/v2")));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpeechMatrixManager v1"));
            }

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/speechmatics-websocket")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocketManager = app.ApplicationServices.GetService<SocketManager>();
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        //await webSocketManager.StartWebSocketAsync(audio);
                        //await webSocketManager.StartWebSocketAsync(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            // Retrieve the WebSocketService instance and start the connection
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var webSocketService = scope.ServiceProvider.GetRequiredService<PingPongMechanism>();
            //    webSocketService.StartWebSocketConnection().Wait();
            //}

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var webSocketService = scope.ServiceProvider.GetRequiredService<PingPongMechanism>();
                 webSocketService.StartWebSocketConnection();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowOrigin");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


    }
}
