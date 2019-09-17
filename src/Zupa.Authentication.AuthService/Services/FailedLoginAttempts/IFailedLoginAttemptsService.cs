using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.Services.FailedLoginAttempts
 {
     public interface IFailedLoginAttemptsService
     {
        Task<bool> HasTooManyRequestsForIpAddressAsync(string ipAddress);
        Task HandleFailedLoginAttempt(string ipAddress);
     }
 }
