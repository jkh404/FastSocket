using System;
using System.Net;
using System.Text;
using MemoryPack;

namespace FastSocket.Udp
{
    public static class IUdpSessionExtension
    {

        public static IUdpSession Send(this IUdpSession udpClient, string msg)
        {

            var data=UTF8Encoding.UTF8.GetBytes(msg);
            udpClient.Send(data);
            return udpClient;
        }
        public static IUdpSession SendPackage<T>(this IUdpSession udpClient, T Package, MemoryPackSerializerOptions? options = null)
        {

            var data=MemoryPackSerializer.Serialize(Package, options);
            udpClient.Send(data);
            return udpClient;
        }
        public static IUdpSession SendPackage<T>(this IUdpSession udpClient,IPEndPoint? iPEndPoint, T Package, MemoryPackSerializerOptions? options = null)
        {
            
            var data = MemoryPackSerializer.Serialize(Package, options);
            udpClient.Send(data, iPEndPoint);
            return udpClient;
        }
        public static T ToPackage<T>(this ReadOnlySpan<byte> bytes)
        {
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
}
