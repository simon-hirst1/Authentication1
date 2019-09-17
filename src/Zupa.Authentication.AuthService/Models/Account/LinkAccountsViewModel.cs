using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Zupa.Authentication.AuthService.Models.Account
{
    public class LinkAccountsViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
        public bool ShowRemoveButton { get; set; }
    }
}
