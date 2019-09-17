using System.ComponentModel.DataAnnotations;

namespace Zupa.Authentication.AuthService.Models.Admin
{
    public class ApiResourceViewModel
    {
        [Required(ErrorMessage = "Name is a required field")]
        [Display(Name = "Name", Prompt = "Resource.Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Display Name is a required field")]
        [Display(Name = "Display Name", Prompt = "Display Name")]
        public string DisplayName { get; set; }
    }
}
