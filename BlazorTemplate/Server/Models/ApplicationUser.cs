using Microsoft.AspNetCore.Identity;

namespace BlazorTemplate.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string CustomClaim { get; set; }
    }
}
