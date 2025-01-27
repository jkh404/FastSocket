using System;
using System.Net;

namespace FastSocket.Tcp
{
    public interface IBaseTcpClient
    {

        public bool Connected { get; }
        public int Available { get; }
        public Guid ChannelFlag { get; }

        public IPEndPoint LocalIPEndPoint { get; }
        public IPEndPoint RemoteIPEndPoint { get; }
        //public TCPBaseOption Option { get; }

    }
}
