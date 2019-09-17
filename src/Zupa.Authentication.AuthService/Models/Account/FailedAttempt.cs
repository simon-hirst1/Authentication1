using System;

namespace Zupa.Authentication.AuthService.Models.Account
{
    public class FailedAttempt
    {
        public string IpAddress { get; }
        public DateTimeOffset FailedAt { get; }
        
        public FailedAttempt(string ipAddress, DateTimeOffset failedAt)
        {
            IpAddress = ipAddress;
            FailedAt = failedAt;
        }
    }
}
