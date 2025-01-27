using System;
using System.Runtime.Serialization;


namespace FastSocket.Tcp
{
    [Serializable]
    public class TcpServerIsStartException : Exception
    {
        public TcpServerIsStartException()
        {
        }

        public TcpServerIsStartException(string message) : base(message)
        {
        }

        public TcpServerIsStartException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TcpServerIsStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
