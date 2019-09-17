namespace Zupa.Authentication.AuthService.Configuration
{
    public class FailedLoginAttemptsSettings
    {
        public int MaxRequestsCount { get; set; }
        public int TimeIntervalInSeconds { get; set; }
    }
}
