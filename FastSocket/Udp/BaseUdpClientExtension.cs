using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace FastSocket.Udp
{
    public static class BaseUdpClientExtension
    {
        public const int AnyPort = IPEndPoint.MinPort;
        public static readonly IPEndPoint Any = new IPEndPoint(IPAddress.Any, AnyPort);
        public static readonly IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, AnyPort);
        public const int MaxUDPSize = 0x10000;
        public static void FastReceive(this UdpClient udpClient, [NotNull] byte[] _buffer, out int receivedLength, out IPEndPoint? remoteEP, AddressFamily _family = AddressFamily.InterNetwork)
        {
            var socket = udpClient.Client;
            EndPoint tempRemoteEP = _family == AddressFamily.InterNetwork ? Any : IPv6Any;
            receivedLength= socket.ReceiveFrom(_buffer, MaxUDPSize, 0, ref tempRemoteEP);
            remoteEP=tempRemoteEP as IPEndPoint;
        }

        
    }
}
