using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Account
{
    public class LogoutInputModel
    {
        [Required]
        public string LogoutId { get; set; }
    }
}
