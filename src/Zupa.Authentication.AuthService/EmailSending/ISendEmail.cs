using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.EmailSending
{
    public interface ISendEmail
    {
        Task SendEmailAsync(OutgoingEmail outgoingEmail);
    }
}
