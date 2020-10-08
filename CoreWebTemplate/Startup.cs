using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CoreWebTemplate {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            // Configure MVC
            
            #if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(opt => {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            #else
            services.AddControllersWithViews().AddJsonOptions(opt => {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            #endif

            services.AddRazorPages();

            var config = Program.ServerConfig;

            // Configure OAuth
            var auth = config.OAuth;
            services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "OAuth";
                })
                .AddCookie()
                .AddOAuth("OAuth", options => {
                    options.ClientId = auth.ClientId;
                    options.ClientSecret = auth.ClientSecret;
                    options.CallbackPath = new PathString("/oauth");

                    options.AuthorizationEndpoint = auth.AuthorizationEndpoint;
                    options.TokenEndpoint = auth.TokenEndpoint;
                    options.UserInformationEndpoint = auth.UserInformationEndpoint;
                    auth.Scopes.ForEach(s => options.Scope.Add(s));
                    
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, auth.IdentityKey);

                    options.Events = new OAuthEvents {
                        OnTicketReceived = context => {
                            context.Properties.IsPersistent = true;
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(auth.AuthExpiration);

                            return Task.FromResult(0);
                        },
                        OnCreatingTicket = async context => {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue(auth.AuthorizationHeaderKey, context.AccessToken);
                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseContentRead, context.HttpContext.RequestAborted);
                            if (auth.DumpOAuthUserInformation) {
                                Log.Information(await response.Content.ReadAsStringAsync());
                            }

                            response.EnsureSuccessStatusCode();

                            var user = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                            context.RunClaimActions(user.RootElement);
                        }
                    };
                });


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


            var config = Program.ServerConfig;

            if (config.BypassCors) {
                app.UseCors();
            }

            if (config.StaticFiles != null) {
                Log.Information($"Serve static files from {config.StaticFiles}");
                var provider = new PhysicalFileProvider(config.StaticFiles);
                app.UseDefaultFiles(new DefaultFilesOptions {
                    FileProvider = provider,
                    RequestPath = ""
                });
                app.UseStaticFiles(new StaticFileOptions {
                    FileProvider = provider,
                    RequestPath = ""
                });
            }

            app.UseAuthentication();
            app.UseSession();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                // for SPA
                if (config.StaticFiles != null) {
                    endpoints.MapFallbackToFile("index.html", new StaticFileOptions {
                        FileProvider = new PhysicalFileProvider(config.StaticFiles)
                    });
                }
            });
        }
    }
}
