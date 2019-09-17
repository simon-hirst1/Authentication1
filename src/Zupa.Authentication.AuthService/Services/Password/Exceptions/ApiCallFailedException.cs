using System;
using System.Runtime.Serialization;

namespace Zupa.Authentication.AuthService.Services.Password
{
    [Serializable]
    public class ApiCallFailedException : Exception
    {
        public ApiCallFailedException() { }

        public ApiCallFailedException(string message) : base(message) { }

        public ApiCallFailedException(string message, Exception innerException) : base(message, innerException) { }

        protected ApiCallFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
