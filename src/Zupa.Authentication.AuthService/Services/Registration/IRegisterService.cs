using System;
using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.Services.Registration
{
    public interface IRegisterService
    {
        Task SendUserRegistrationAsync(Guid userId, string userName, string email);
    }
}
