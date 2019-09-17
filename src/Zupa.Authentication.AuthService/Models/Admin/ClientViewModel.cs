using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Admin
{
    public class ClientViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client ID is a required field")]
        [Display(Name = "Client ID", Prompt = "Client.Name")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "Client Name is a required field")]
        [Display(Name = "Client Name", Prompt = "Client Name")]
        public string ClientName { get; set; }

        [Required]
        [Display(Name = "Allow access tokens via the browser?")]
        public bool AllowAccessTokensViaBrowser { get; set; }

        [Required(ErrorMessage = "Cors Origin is a required field")]
        [Display(Name = "Cors Origin", Prompt = "http://localhost:5000")]
        public string CorsOrigin { get; set; }

        [Required(ErrorMessage = "Redirect Uri is a required field")]
        [Display(Name = "Redirect Uri", Prompt = "http://localhost:5000/callback.html")]
        public string RedirectUri { get; set; }

        [Required(ErrorMessage = "Post Logout Redirect Uri is a required field")]
        [Display(Name = "Post Logout Redirect Uri", Prompt = "http://localhost:5000/index.html")]
        public string PostLogoutRedirectUri { get; set; }

        [Required(ErrorMessage = "Scopes is a required field")]
        [Display(Name = "API Scopes", Prompt = "Resource.Id Resource2.Id Resource3.Id")]
        public string ApiScopes { get; set; }
    }
}
