using Microsoft.Azure.Cosmos.Table;

namespace Zupa.Authentication.AuthService.Models.Entity
{
    public class WhitelistEntity : TableEntity
    {
        public string EmailAddress { get; set; }
    }
}
