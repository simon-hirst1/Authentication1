using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Account
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
