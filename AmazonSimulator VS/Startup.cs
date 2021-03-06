﻿using Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.WebSockets;
using System.Threading;
using Views;

namespace AmazonSimulator_VS
{
    public class Startup
    {
        public static SimulationController simulationController;

        public Startup(IConfiguration configuration)
        {
            simulationController = new SimulationController(new Models.World());

            Thread InstanceCaller = new Thread(
                new ThreadStart(simulationController.Simulate));

            // Start the thread.
            InstanceCaller.Start();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();

            var provider = new FileExtensionContentTypeProvider();

            // Add new mappings
            provider.Mappings[".mtl"] = "text/plain";
            provider.Mappings[".glb"] = "model/gltf-binary";
            provider.Mappings[".exr"] = "image/x-exr";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            WebSocketOptions options = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMilliseconds(500),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(options);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/connect_client")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        Console.WriteLine("Accepting Websocket...");
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        ClientView clientView = new ClientView(webSocket);
                        simulationController.AddView(clientView);

                        clientView.Run();
                        
                        simulationController.RemoveView(clientView);
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

            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            //app.UseMvc();

            //app.UseDirectoryBrowser(new DirectoryBrowserOptions());
        }
    }
}
