using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Zupa.Authentication.AuthService.Exceptions
{
    public class IpAddressNotFoundException : Exception
    {
        public IpAddressNotFoundException() : base("Ip address not found.") { }

        protected IpAddressNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
