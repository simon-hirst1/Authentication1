using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.Models.Account;

namespace Zupa.Authentication.AuthService.Services.FailedLoginAttempts
{
    public class FailedLoginAttemptsService : IFailedLoginAttemptsService
    {
        private readonly int _maxRequestsCount;
        private readonly int _timeIntervalInSeconds;
        private readonly IFailedLoginAttemptsRepository _repository;

        public FailedLoginAttemptsService(
            IFailedLoginAttemptsRepository repository,
            IOptions<FailedLoginAttemptsSettings> failedAttemptsSettings
        ) {
            _maxRequestsCount = failedAttemptsSettings.Value.MaxRequestsCount;
            _timeIntervalInSeconds = failedAttemptsSettings.Value.TimeIntervalInSeconds;
            _repository = repository;
        }
        
        public async Task<bool> HasTooManyRequestsForIpAddressAsync(string ipAddress)
        {
            var dateToCheckAgainst = DateTimeOffset.Now.AddSeconds(-_timeIntervalInSeconds);
            var requests = await _repository.FindFailedAttemptsForIpAddressAsync(ipAddress, dateToCheckAgainst);

            return requests.Count > _maxRequestsCount;
        }

        public async Task HandleFailedLoginAttempt(string ipAddress)
        {
            await RemoveAnyOldEntries(ipAddress);
            await PersistFailedAttemptAsync(ipAddress);
        }
        
        private async Task PersistFailedAttemptAsync(string ipAddress)
        {
            var failedAttempt = new FailedAttempt(ipAddress, DateTimeOffset.Now);
            
            try
            {
                await _repository.PersistFailedAttemptAsync(failedAttempt);
            }
            catch (FailedAttemptConflictException exception)
            {
                throw new FailedAttemptExistsException(failedAttempt.IpAddress, failedAttempt.FailedAt, exception);
            }
        }

        private async Task RemoveAnyOldEntries(string ipAddress)
        {
            var olderThan = DateTimeOffset.Now.AddSeconds(-_timeIntervalInSeconds);
            await _repository.RemoveAnyOldEntries(ipAddress, olderThan);
        }
    }
}
