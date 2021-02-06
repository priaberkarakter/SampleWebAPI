using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;


namespace DenBagus
{
    public class Startup
    {
        IServiceCollection services;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.services = services;

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AllowSynchronousIO = true;
            });


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add Kendo UI services to the services container
            //services.AddKendo();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            //.AddOpenIdConnect("OpenIdConnect", "Epson OpenID Connect", options =>
            //{
            //    // URL of the Keycloak server
            //    options.Authority = Configuration["auth:authority"];
            //    // Client configured in the Keycloak
            //    options.ClientId = Configuration["auth:clientId"];
            //    // Client secret shared with Keycloak
            //    options.ClientSecret = Configuration["auth:clientSecret"];
                
            //    // Define scope
            //    options.Scope.Add("openid");
            //    options.Scope.Add("email");
            //    options.Scope.Add("profile");
            //    options.Scope.Add("roles");
                
            //    // OpenID flow to use
            //    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            //    options.CallbackPath = "/signin-keycloak";
            //    options.SaveTokens = true;
            //    options.GetClaimsFromUserInfoEndpoint = true;
                
            //    if (bool.TryParse(Configuration["auth:requireHttpsMetadata"], out var requireHttpsMetadata))
            //    {
            //        options.RequireHttpsMetadata = requireHttpsMetadata;
            //    }
            //})
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = Configuration["auth-api:authority"];
                o.IncludeErrorDetails = true;
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["auth-api:authority"],
                    ValidateLifetime = true
                };
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "text/plain";
                        return c.Response.WriteAsync(c.Exception.ToString());
                    }
                };
            });

            services.AddAuthorization();           

            services.AddApiVersioning(config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            services.AddControllersWithViews(options =>
            {
                //options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
            })
            .AddDataAnnotationsLocalization()
            .AddNewtonsoftJson(setupAction => setupAction.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            var connectionString = Configuration.GetConnectionString("VendorProdDb");
            services.AddDbContext<DataAPI>(options => options
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                var knownProxies = Configuration["ReverseProxy:KnownProxies"];
                var forwardLimit = 1;
                int.TryParse(Configuration["ReverseProxy:ForwardLimit"], out forwardLimit);
                options.ForwardLimit = forwardLimit;
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
                if (!string.IsNullOrEmpty(knownProxies))
                {
                    var proxies = knownProxies.Split(new char[] { ',' });
                    foreach (var proxy in proxies)
                    {
                        if (IPAddress.TryParse(proxy, out var ipAddress))
                        {
                            options.KnownProxies.Add(ipAddress);
                        }
                    }
                }
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Configuration for Telerik Reporting
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddRazorPages()
            .AddNewtonsoftJson();

            services.AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                })
                .AddMvcOptions(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseQueryStrings = true;
            });


            services.AddSingleton<ConfigurationService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //this.services.AddTransient(ctx => new Controllers.ReportsController(new ConfigurationService(env)));

            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class ConfigurationService
    {
        public IConfiguration Configuration { get; private set; }

        public IWebHostEnvironment Environment { get; private set; }
        public ConfigurationService(IWebHostEnvironment environment)
        {
            this.Environment = environment;

            var configFileName = System.IO.Path.Combine(environment.ContentRootPath, "appsettings.json");
            var config = new ConfigurationBuilder()
                            .AddJsonFile(configFileName, true)
                            .Build();

            this.Configuration = config;
        }
    }

}
