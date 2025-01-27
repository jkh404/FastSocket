using System;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{
    public interface IChannelEventHandler
    {
        public delegate void ReceiveEventHandler(IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData);
        public delegate void LeaveEventHandler(IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData);
        public ReceiveEventHandler? OnReceive { get; set; }
        public LeaveEventHandler? OnLeave { get; set; }
    }
}
