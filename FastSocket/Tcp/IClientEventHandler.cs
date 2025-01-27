using System;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{
    public interface IClientEventHandler
    {
        public delegate void ReceiveEventHandler(IBaseTcpClient self,PackageDataType dataType, ReadOnlySpan<byte> dataPacketData);
        public delegate void ServerStopEventHandler(IBaseTcpClient self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData);
        public ReceiveEventHandler? OnReceive {  get; set; }
        public ServerStopEventHandler? OnServerStop {  get; set; }
    }
}
