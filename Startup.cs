using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.FileProviders;

namespace BlazorApp1
{
    public class TrackingCircuitHandler : CircuitHandler
    {
        private HashSet<Circuit> circuits = new HashSet<Circuit>();

        public override Task OnConnectionUpAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            //circuits.Add(circuit);

            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            //circuits.Remove(circuit);

            AppLifetime?.StopApplication();
            return Task.CompletedTask;
        }

        //public int ConnectedCircuits => circuits.Count;

        public static TrackingCircuitHandler Instance { get; }=new TrackingCircuitHandler();
        public Microsoft.Extensions.Hosting.IHostApplicationLifetime AppLifetime;
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<CircuitHandler>(TrackingCircuitHandler.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.Extensions.Hosting.IHostApplicationLifetime appLifetime)
        {
            TrackingCircuitHandler.Instance.AppLifetime = appLifetime;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            //Tips: for embedded mode
            var ass = Assembly.GetEntryAssembly();
            var resourceAssemblyName = ass.GetName().Name;
            app.UseStaticFiles(new StaticFileOptions
                {
                    //[Tips] resourceAssemblyName is something like project name?
                    FileProvider = new EmbeddedFileProvider(ass, $"{resourceAssemblyName}.wwwroot"),
                    RequestPath = "/embeded"
                }
            );

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");

            });

            appLifetime.ApplicationStarted.Register(() => OpenBrowser(
                app.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.First()));

        }

        //[TODO]
        private static void OpenBrowser(string url)
        {

            //TODO what's the safe way to detect the browser is closed and close the main application manualy?
            Process.Start(
                new ProcessStartInfo("cmd", $"/c start {url}")
                {
                    CreateNoWindow = true
                });


            // var p = new Process{
            //     StartInfo = new ProcessStartInfo("cmd", $"/c start {url}")
            //     {
            //         CreateNoWindow = true
            //     }
            // };
            // p.Start();
            //
            // Task.Factory.StartNew(new Action(() =>
            // {
            //     p.WaitForExit();
            //     System.Environment.Exit(0);
            // }));
        }
    }
}
