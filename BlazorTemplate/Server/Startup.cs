using BlazorTemplate.Server.Data;
using BlazorTemplate.Server.GrpcServices;
using BlazorTemplate.Server.Models;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using IdentityServer4.Services;

namespace BlazorTemplate.Server
{
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>() // For Roles, see (client side) /RolesClaimsPrincipalFactory.cs
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory
                >(); // To add a custom claim for the user, see: /Models/ApplicationUser.cs

            var alexaVendor = Configuration["Alexa:BlazorNews:VendorId"];
            var alexaSecretText = "AlexaBlazorNewsSecret"; // I use this secret under the Alexa configuration.
            var client = new IdentityServer4.Models.Client
            {
                ClientId = "AlexaBlazorNews",
                ClientName = "AlexaBlazorNews",
                Enabled = true,
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
                RequirePkce = false,
                RequireClientSecret = true,
                AllowRememberConsent = true,
                ClientSecrets = {new Secret(alexaSecretText.Sha256()) },
                RedirectUris =
                {
                    "https://pitangui.amazon.com/api/skill/link/" + alexaVendor,
                    "https://layla.amazon.com/api/skill/link/" + alexaVendor,
                    "https://alexa.amazon.co.jp/api/skill/link/"+alexaVendor
                },
                PostLogoutRedirectUris =
                {
                    "https://pitangui.amazon.com/api/skill/link/" + alexaVendor,
                    "https://layla.amazon.com/api/skill/link/" + alexaVendor,
                    "https://alexa.amazon.co.jp/api/skill/link/"+alexaVendor
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Phone,
                    "alexa"
                },
                AllowOfflineAccess = true,
                AccessTokenType = AccessTokenType.Jwt,
                
            };

            var clients = new List<IdentityServer4.Models.Client>();
            var configClients = Configuration.GetSection("IdentityServer:Clients")
                .Get<IdentityServer4.Models.Client[]>();

            clients.Add(client);
            clients.AddRange(configClients);

            ///
            // This ...
            ///
            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
                {
                    options.IdentityResources["openid"].UserClaims.Add("role"); // Roles
                    options.ApiResources.Single().UserClaims.Add("role");
                    options.IdentityResources["openid"].UserClaims.Add("application_db_user_id"); // Custom Claim
                    options.ApiResources.Single().UserClaims.Add("application_db_user_id");
                    options.IdentityResources["openid"].UserClaims.Add("email");
                    options.ApiResources.Single().UserClaims.Add("email");
                    options.IdentityResources["openid"].UserClaims.Add("name");
                    options.ApiResources.Single().UserClaims.Add("name");
                    options.Clients.AddRange(clients.ToArray()); // added clients. trying to add alexa client as well
                });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            ///
            // TODO: Or this (Use a Profile Service)
            // Roles seem to work, Client displays them, but I can't add a Role: 403 Forbidden, Test with gRPC Authorization (Role=Administrators) as well.
            // See: https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?tabs=visual-studio#profile-service
            ///
            //services.AddIdentityServer()
            //    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();
            services.AddTransient<IProfileService, ProfileService>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllersWithViews().AddNewtonsoftJson();
            //.AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            //    options.JsonSerializerOptions.PropertyNamingPolicy = null;
            //});


            services.AddRazorPages();
            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext ctx, IServiceProvider serviceProvider)
        {
            new SeedData(ctx).CreateUserAndRoles(serviceProvider).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseGrpcWeb();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrators" })
                    .EnableGrpcWeb();

                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
