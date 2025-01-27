using System;
using System.Net;

namespace FastSocket.Tcp
{
    public interface ITcpClient : IBaseReadOnlyTcpClient, IBaseWriteOnlyTcpClient, IDisposable
    {
        public bool Connect(string host, int port);
        //public void Connect(IPEndPoint iPEndPoint);
        public bool Connect(IPEndPoint iPEndPoint);
        public void Disconnect();
    }
}
