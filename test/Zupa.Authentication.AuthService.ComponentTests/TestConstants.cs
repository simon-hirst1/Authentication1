using System;

namespace Zupa.Authentication.AuthService.ComponentTests
{
    public class TestConstants
    {
        public const string FailedAttemptsConnectionString = "UseDevelopmentStorage=true";
        public static readonly string FailedAttemptsTableName = $"FailedAttemptsComponentTests{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
        
        public const int MaxRequestsCount = 5;
        public const int TimeIntervalInSeconds = 600;
        
        public const string FakeIpAddress = "1.2.3.4";
    }
}
