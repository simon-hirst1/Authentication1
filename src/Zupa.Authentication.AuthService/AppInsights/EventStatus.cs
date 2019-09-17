namespace Zupa.Authentication.AuthService.AppInsights
{
    public enum EventStatus
    {
        None,
        Success,
        Fail,
        Lockout,
        NotVerified,
        UserNotFound,
        InvalidParameter,
        ExternalLoginInfoNotFound,
        ParameterNotFound
    }
}
