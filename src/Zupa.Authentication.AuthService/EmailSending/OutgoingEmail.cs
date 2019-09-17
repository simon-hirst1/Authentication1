using System;

namespace Zupa.Authentication.AuthService.EmailSending
{
    [Serializable]
    public class OutgoingEmail
    {
        public string To { get; set; }
        public string From { get; set; }
        public Template Template { get; set; }
    }
}
