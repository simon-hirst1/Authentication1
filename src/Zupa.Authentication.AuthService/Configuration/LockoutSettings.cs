namespace Zupa.Authentication.AuthService.Configuration
{
    public class LockoutSettings
    {
        public int MaxFailedAccessAttempts { get; set; }

        public int DefaultLockoutTimeSpan { get; set; }
    }
}
