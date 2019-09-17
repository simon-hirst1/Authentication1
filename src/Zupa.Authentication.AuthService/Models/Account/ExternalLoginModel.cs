using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Account
{
    public class ExternalLoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
