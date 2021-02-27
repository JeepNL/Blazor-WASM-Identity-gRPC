using BlazorTemplate.Server.Data;
using BlazorTemplate.Server.GrpcServices;
using BlazorTemplate.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

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
				.AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>(); // To add a custom claim for the user, see: /Models/ApplicationUser.cs

			services.AddIdentityServer()
				.AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
				{
					options.IdentityResources["openid"].UserClaims.Add("role"); // Roles
					options.ApiResources.Single().UserClaims.Add("role");
					options.IdentityResources["openid"].UserClaims.Add("custom_claim"); // Custom Claim
					options.ApiResources.Single().UserClaims.Add("custom_claim");
				});
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

			services.AddAuthentication()
				.AddIdentityServerJwt();

			services.AddDatabaseDeveloperPageExceptionFilter();

			services.AddControllersWithViews();
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

			//app.UseGrpcWeb();
			app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // No need for .EnableGrpcWeb() below.

			app.UseIdentityServer();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<GreeterService>()
					.RequireAuthorization(new AuthorizeAttribute { Roles = "Administrators" });
				//.EnableGrpcWeb(); // See above: "app.UseGrpcWeb(..."

				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}
