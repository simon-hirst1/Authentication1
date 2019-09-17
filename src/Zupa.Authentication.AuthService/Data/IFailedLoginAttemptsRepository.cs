using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Models.Account;

namespace Zupa.Authentication.AuthService.Data
{
    public interface IFailedLoginAttemptsRepository
    {
        Task<List<FailedAttempt>> FindFailedAttemptsForIpAddressAsync(string ipAddress, DateTimeOffset newerThan);
        Task PersistFailedAttemptAsync(FailedAttempt failedAttempt);
        Task RemoveAnyOldEntries(string ipAddress, DateTimeOffset olderThan);
    }
}
