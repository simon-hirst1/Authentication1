using System;
using System.Runtime.Serialization;

namespace Zupa.Authentication.AuthService.Data
{
    [Serializable]
    public class FailedAttemptConflictException : Exception
    {
        public FailedAttemptConflictException(string message, Exception inner) : base(message, inner) {}
        
        protected FailedAttemptConflictException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
