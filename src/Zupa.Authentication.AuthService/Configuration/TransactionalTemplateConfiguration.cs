namespace Zupa.Authentication.AuthService.Configuration
{
    public class TransactionalTemplateConfiguration
    {
        public string RegistrationTemplate { get; set; }
        public string LockoutTemplate { get; set; }
        public string WelcomeTemplate { get; set; }
        public string ResetPasswordTemplate { get; set; }
        public string FromEmail { get; set; }
        public string UserNameKey { get; set; }
        public string EmailKey { get; set; }
        public string LinkKey { get; set; }
        public string LockoutTimeKey { get; set; }
        public string DynamicUserNameKey { get; set; }
        public string DynamicLinkKey { get; set; }
    }
}
