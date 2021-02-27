using BlazorTemplate.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorTemplate.Server
{
	public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
	{
		public AppClaimsPrincipalFactory(
			UserManager<ApplicationUser> userManager
			, RoleManager<IdentityRole> roleManager
			, IOptions<IdentityOptions> optionsAccessor)
		: base(userManager, roleManager, optionsAccessor)
		{ }

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
		{
			var identity = await base.GenerateClaimsAsync(user);

			if (!string.IsNullOrWhiteSpace(user.CustomClaim))
			{
				identity.AddClaim(new Claim("custom_claim", user.CustomClaim));
			}

			return identity;
		}
	}
}
