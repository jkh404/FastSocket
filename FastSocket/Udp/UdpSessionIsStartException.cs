using System;
using System.Runtime.Serialization;

namespace FastSocket.Udp
{
    [Serializable]
    public class UdpSessionIsStartException : Exception
    {
        public UdpSessionIsStartException()
        {
        }

        public UdpSessionIsStartException(string message) : base(message)
        {
        }

        public UdpSessionIsStartException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UdpSessionIsStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
