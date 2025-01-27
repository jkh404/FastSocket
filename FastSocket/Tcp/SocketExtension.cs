using System;
using System.Net.Sockets;


namespace FastSocket.Tcp
{
    internal static class SocketExtension
    {
        internal static void ReceiveALL(this Socket socket,Span<byte> buffer)
        {
            var AllLen= buffer.Length;
            var readLen = 0;
            while (readLen < AllLen)
            {
                readLen+= socket.Receive(buffer.Slice(readLen, AllLen-readLen));
            }
        }
        internal static void SendALL(this Socket socket, ReadOnlySpan<byte> buffer)
        {
            if(socket==null)throw new ArgumentNullException(nameof(socket));
            var AllLen = buffer.Length;
            var readLen = 0;
            while (readLen < AllLen)
            {
                readLen+= socket.Send(buffer.Slice(readLen, AllLen-readLen));
            }
        }
    }
}
