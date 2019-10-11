using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wp.device.CoreWebService
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


        }

        /// <summary>
        /// Electron ´°¿ÚÆô¶¯
        /// </summary>
        /// <returns></returns>
        public async Task BrowserWindow()
        {
            var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Width = 800,
                Height = 500,
                Show = true,
                Center = true,
                AutoHideMenuBar = true,
                Fullscreen = false,
                Transparent = true,
                HasShadow=true,
                Title = "²å¼þ²âÊÔ",

                WebPreferences = new WebPreferences()
                {
                    WebSecurity = false
                },
                Icon = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"favicon.ico")}",
            }, $"http://localhost:8080/");

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle("²å¼þ²âÊÔ02");

            browserWindow.Show();

            //// 3s
            //var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            //{
            //    Width = width,
            //    Height = height,
            //    Show = false,
            //    Frame = false,              // Òþ²Ø´°¿Ú
            //    AutoHideMenuBar = true,     // Òþ²Ø²Ëµ¥
            //    Transparent = true,         // ´°ÌåÍ¸Ã÷
            //    WebPreferences = new WebPreferences()
            //    {
            //        WebSecurity = false
            //    },
            //    Icon = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IcoDefintion.Logo32_32)}",
            //    Title = title,
            //}, $"http://localhost:{BridgeSettings.WebPort}/app?v={DateTime.Now.Ticks}");
            //WindowDefintion.SetWindowId(EnumWindowType.MainWindow, browserWindow.Id);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            // Electron Support
            if (HybridSupport.IsElectronActive)
                Task.Run(async () => { await BrowserWindow(); });
        }
    }
}
