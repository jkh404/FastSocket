using System;
using System.Net;

namespace FastSocket.Udp
{
    public interface IUdpSession:IDisposable
    {
        public bool IsStart { get;  }

        public delegate void ReceiveEventHandler(IUdpSession self, IPEndPoint remoteEP, ReadOnlySpan<byte> bytes);
        public ReceiveEventHandler? OnReceive { get; set; }
        public IUdpSession Connect(string hostname, int port);
        public IUdpSession Connect(IPEndPoint iPEndPoint);
        public void JoinMulticastGroup(IPAddress iPAddress);
        public void JoinMulticastGroup(IPAddress group, IPAddress mcint);
        void DropMulticastGroup(IPAddress iPAddress);


        public void Send(ReadOnlySpan<byte> bytes, IPEndPoint? iPEndPoint = null);

        public void Start(bool asyncReceive = false);
        public void Stop();
    }
}
