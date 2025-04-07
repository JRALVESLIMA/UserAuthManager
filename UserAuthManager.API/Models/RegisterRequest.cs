using System.ComponentModel.DataAnnotations;

namespace UserAuthManager.API.Models
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
