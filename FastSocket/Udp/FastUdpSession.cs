using System;
using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace FastSocket.Udp
{

    public class FastUdpSession : IUdpSession
    {
        public readonly UdpClient m_udpClient;

        private readonly byte[] m_buffer = new byte[BaseUdpClientExtension.MaxUDPSize];
        
        private bool m_isStart=false;
        private bool m_isStop=true;
        private object m_lockStartObj = new object();

        public bool IsStart { get => m_isStart && !m_isStop; }

        public IUdpSession.ReceiveEventHandler? OnReceive { get; set; }
        public FastUdpSession(UdpClient udpClient)
        {
            m_udpClient=udpClient;
            m_udpClient.EnableBroadcast = true;
            //m_udpClient.MulticastLoopback=false;
        }
        public FastUdpSession(int port, AddressFamily family = AddressFamily.InterNetwork)
        {
            m_udpClient=new UdpClient(port, family);
            m_udpClient.EnableBroadcast = true;
            //m_udpClient.MulticastLoopback=false;
        }
        public FastUdpSession(IPEndPoint iPEndPoint)
        {
            m_udpClient=new UdpClient(iPEndPoint);
            //m_udpClient.MulticastLoopback=false;

            m_udpClient.EnableBroadcast = true;
        }

        public IUdpSession Connect(string hostname, int port)
        {
            m_udpClient.Connect(hostname, port);
            return this;
        }

        public IUdpSession Connect(IPEndPoint iPEndPoint)
        {
            m_udpClient.Connect(iPEndPoint);
            return this;
        }


        public void Start(bool asyncReceive = false)
        {
            if (m_isStart) throw new UdpSessionIsStartException();
            lock (m_lockStartObj)
            {
                if (m_isStart) throw new UdpSessionIsStartException();
                m_isStart=true;
                m_isStop=false;
            }
            Task.Run(() => {
                try
                {
                    while (m_isStart && !m_isStop)
                    {
                        if (asyncReceive)
                        {
                            m_udpClient.FastReceive(m_buffer, out var receivedLength, out var remoteEP);
                            var resultBytes = ArrayPool<byte>.Shared.Rent(receivedLength);
                            m_buffer.AsSpan(0, receivedLength).CopyTo(resultBytes);
                            Task.Run(() =>
                            {
                                OnReceive?.Invoke(this, remoteEP!, resultBytes.AsSpan(0, receivedLength));
                                ArrayPool<byte>.Shared.Return(resultBytes);
                            });

                        }
                        else
                        {
                            m_udpClient.FastReceive(m_buffer, out var receivedLength, out var remoteEP);
                            OnReceive?.Invoke(this, remoteEP!, m_buffer.AsSpan(0, receivedLength));
                        }
                    }
                }
                catch 
                {
                    Stop();
                }
               
            });
        }
        protected int Send(IPEndPoint iPEndPoint, byte[] bytes, int length = 0)
        {
            if (length<=0) return m_udpClient.Send(bytes, bytes.Length, iPEndPoint);
            else return m_udpClient.Send(bytes, length, iPEndPoint);

        }
        protected int Send(byte[] bytes, int length = 0)
        {
            if (length<=0) return m_udpClient.Send(bytes, bytes.Length);
            else return m_udpClient.Send(bytes, length);
        }

        public void Send(ReadOnlySpan<byte> bytes, IPEndPoint? iPEndPoint = null)
        {
            var len = bytes.Length;
            int okSent = 0;
#if NETSTANDARD2_1
            byte[] buffer = bytes.ToArray();
#endif
            if (iPEndPoint==null)
            {
                while (okSent < len)
                {

#if NETSTANDARD2_1
                    okSent+=m_udpClient.Send(buffer[okSent..(len-1)], len-okSent);
                    
#elif NET6_0_OR_GREATER
                    okSent+=m_udpClient.Send(bytes.Slice(okSent, len-okSent));
#endif
                }
            }
            else
            {
                while (okSent < len)
                {
#if NETSTANDARD2_1
                    okSent+=m_udpClient.Send(buffer[okSent..(len-1)], len-okSent,iPEndPoint);
                    
#elif NET6_0_OR_GREATER
                    okSent+=m_udpClient.Send(bytes.Slice(okSent, len-okSent), iPEndPoint);
#endif
                }
            }
        }

        public void JoinMulticastGroup(IPAddress iPAddress)
        {

            var _family = m_udpClient.Client.AddressFamily;

            
            if (_family == AddressFamily.InterNetwork)
            {
                IReadOnlyDictionary<int,string> Ips = NetworkInterfaceUtil.GetLocalIPv4s();
                foreach (var item in Ips)
                {
                    try
                    {
                        JoinMulticastGroup(iPAddress, IPAddress.Parse(item.Value));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {

                m_udpClient.Client.SetSocketOption(
                    SocketOptionLevel.IPv6,
                    SocketOptionName.AddMembership,
                    new IPv6MulticastOption(iPAddress));
            }
            
        }
        public void JoinMulticastGroup(IPAddress group, IPAddress mcint)
        {

            
            var _family = m_udpClient.Client.AddressFamily;


            m_udpClient.Client.SetSocketOption(
                                    SocketOptionLevel.IP,
                                    SocketOptionName.AddMembership,
                                    new MulticastOption(group, mcint));

        }
        public void DropMulticastGroup(IPAddress iPAddress)
        {
            var _family = m_udpClient.Client.AddressFamily;
            if (_family == AddressFamily.InterNetwork)
            {
                IReadOnlyDictionary<int, string> Ips = NetworkInterfaceUtil.GetLocalIPv4s();
                foreach (var item in Ips)
                {
                    try
                    {
                        m_udpClient.Client.SetSocketOption(
                            SocketOptionLevel.IP,
                            SocketOptionName.DropMembership,
                    new MulticastOption(iPAddress, item.Key));
                    }
                    catch (Exception)
                    {

                    }
                }
                
            }
            else
            {
                m_udpClient.Client.SetSocketOption(
                    SocketOptionLevel.IPv6,
                    SocketOptionName.DropMembership,
                    new IPv6MulticastOption(iPAddress));
            }
        }
        public void Stop()
        {
            lock (m_lockStartObj)
            {
                m_isStart = false;
                m_isStop=true;
            }
        }
        public void Dispose()
        {
            Stop();
            m_udpClient?.Close();
            m_udpClient?.Dispose();
            OnReceive =null;
        }

    }
}
