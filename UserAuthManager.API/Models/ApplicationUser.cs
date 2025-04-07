using Microsoft.AspNetCore.Identity;

namespace UserAuthManager.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
