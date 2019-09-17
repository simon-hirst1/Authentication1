using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Zupa.Libraries.ServiceBus.ServiceBusClient;

namespace Zupa.Authentication.AuthService.Services.Registration
{
    public class RegisterService : IRegisterService
    {
        private readonly IServiceBusClient<ITopicClient> _topicClient;

        public RegisterService(IServiceBusClient<ITopicClient> topicClient)
        {
            _topicClient = topicClient;
        }

        public async Task SendUserRegistrationAsync(Guid userId, string userName, string email)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException($"{nameof(userId)} cannot be an empty guid");

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException($"{nameof(userName)} cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException($"{nameof(email)} cannot be empty");

            var messageToBeEncoded = new { UserId = userId, UserName = userName, Email = email };

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageToBeEncoded)));

            await _topicClient.SendAsync(message);
        }
    }
}
