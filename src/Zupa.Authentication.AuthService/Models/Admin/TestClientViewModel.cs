using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Admin
{
    public class TestClientViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client ID is a required field")]
        [Display(Name = "Client ID", Prompt = "Client.Name")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "Client Name is a required field")]
        [Display(Name = "Client Name", Prompt = "Client Name")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Client Secret is a required field")]
        [Display(Name = "Client Secret", Prompt = "Secret")]
        public string ClientSecret { get; set; }

        [Required(ErrorMessage = "Scopes is a required field")]
        [Display(Name = "API Scopes", Prompt = "Resource.Id Resource2.Id Resource3.Id")]
        public string ApiScopes { get; set; }
    }
}
