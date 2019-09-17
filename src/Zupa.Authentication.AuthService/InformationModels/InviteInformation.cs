using System;

namespace Zupa.Authentication.AuthService.InformationModels
{
    public class InviteInformation
    {
        public Guid Id { get; }
        public string EmailAddress { get; }

        public InviteInformation(Guid id, string emailAddress)
        {
            Id = id;
            EmailAddress = emailAddress;
        }
    }
}
