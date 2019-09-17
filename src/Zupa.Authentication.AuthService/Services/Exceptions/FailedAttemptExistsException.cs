using System;
using System.Runtime.Serialization;

namespace Zupa.Authentication.AuthService.Services
{
    [Serializable]
    public class FailedAttemptExistsException : Exception
    {
        public FailedAttemptExistsException(string ipAddress, DateTimeOffset failedAt) : base(
            $"Failed attempt with ip address {ipAddress} which occurred at {failedAt.ToString()} already exists.") { }

        public FailedAttemptExistsException(string ipAddress, DateTimeOffset failedAt, Exception inner) : base(
            $"Failed attempt with ip address {ipAddress} which occurred at {failedAt.ToString()} already exists.", 
            inner) { }

        protected FailedAttemptExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
