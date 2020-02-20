using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CoreWebTemplate {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            // Configure MVC
            
            #if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            #else
            services.AddControllersWithViews();
            #endif

            services.AddRazorPages();

            var config = Program.ServerConfig;

            // Session
            services.AddDistributedMemoryCache();
            services.AddSession(option => {
                option.Cookie.Name = "__CoreWebTemplate_session";
                option.IdleTimeout = TimeSpan.FromSeconds(3600);
            });

            // CORS
            if (config.BypassCors) {
                services.AddCors(o => {
                    o.AddDefaultPolicy(builder => {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime appLifetime) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
