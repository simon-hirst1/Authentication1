using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace Zupa.Authentication.AuthService.EmailSending
{
    public class SendGridEmailSender : ISendEmail
    {
        private static ISenderClient _queueClient;

        public SendGridEmailSender(ISenderClient queueClient)
        {
            _queueClient = queueClient;
        }

        public async Task SendEmailAsync(OutgoingEmail outgoingEmail)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(outgoingEmail)));
            await _queueClient.SendAsync(message);
            await _queueClient.CloseAsync();
        }
    }
}
