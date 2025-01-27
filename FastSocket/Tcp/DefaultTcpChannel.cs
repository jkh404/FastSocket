using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FastSocket.Tcp.Package;


namespace FastSocket.Tcp
{
    internal class DefaultTcpChannel : ITcpChannel
    {
        private readonly TcpClient m_tcpClient;
        private readonly Guid m_channelFlag;
        private  bool m_isStartReceive=false;
        private  object m_lockStartReceive=new object();
        private  object m_lockSend=new object();

        private IChannelEventHandler.ReceiveEventHandler? m_OnReceive;
        private IChannelEventHandler.LeaveEventHandler? m_OnLeave;
        public IChannelEventHandler.ReceiveEventHandler? OnReceive { get => m_OnReceive; set {
                if (m_isStartReceive) throw new InvalidOperationException("TcpChannel is Start");
                else m_OnReceive = value;
            } }
        public IChannelEventHandler.LeaveEventHandler? OnLeave { get => m_OnLeave; set {
                if (m_isStartReceive) throw new InvalidOperationException("TcpChannel is Start");
                else m_OnLeave = value;
            } }



        internal DefaultTcpChannel(TcpClient tcpClient, Guid channelFlag)
        {
            m_tcpClient=tcpClient;
            m_channelFlag=channelFlag;
        }

        public bool Connected => m_tcpClient.Connected;

        public int Available => m_tcpClient.Available;

        public bool IsStartReceive => m_isStartReceive;
        public Guid ChannelFlag =>m_channelFlag;
        public IPEndPoint LocalIPEndPoint => (m_tcpClient.Client.LocalEndPoint as IPEndPoint)!;
        public IPEndPoint RemoteIPEndPoint => (m_tcpClient.Client.RemoteEndPoint as IPEndPoint)!;

        public IBaseWriteOnlyTcp Send(ReadOnlySpan<byte> buffer, PackageDataType DataType = PackageDataType.ByteArray)
        {
            
            if (buffer.Length==0) return this;
            lock (m_lockSend)
            {
                PackageHead packageHead = new PackageHead()
                {
                    FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                    ChannelFlag = m_channelFlag,
                    TypeFlag=PackageHeadType.Data,
                    DataLength=buffer.Length,
                    DataType=DataType
                };
                SendALLByNative(packageHead.AsBytes());
                SendALLByNative(buffer);
            }
            return this;
        }
        public IBaseWriteOnlyTcp SendALLByNative(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length==0)return this;
            else
            {
                m_tcpClient.Client.SendALL(buffer);
                return this;
            }
        }

        public bool StartReceive(bool asyncReceive=false)
        {
            if(m_isStartReceive) return false;
            lock (m_lockStartReceive) 
            {
                if (m_isStartReceive) return false;
                m_isStartReceive=true;
            }
            Thread receiveThread = new Thread(() =>
            {
                try
                {
                    Span<byte> buffer = stackalloc byte[FastSocketGlobalConfiguration.PackageHeadSize];
                    while (m_isStartReceive)
                    {
                        m_tcpClient.Client.ReceiveALL(buffer);
                        var head = PackageHeadUtil.ToPackageHead(buffer);
                        if (head.FastSocketFlag==FastSocketGlobalConfiguration.FastSocketFlag
                            && m_channelFlag==head.ChannelFlag)
                        {
                            if (head.TypeFlag==PackageHeadType.Data && head.DataLength>0)
                            {
                                byte[] dataBuffer = ArrayPool<byte>.Shared.Rent(head.DataLength);
                                Span<byte> tempDataBuffer = dataBuffer.AsSpan(0, head.DataLength);
                                m_tcpClient.Client.ReceiveALL(tempDataBuffer);
                                if (asyncReceive)
                                {
                                    Thread postDataThread = new Thread(() =>
                                    {
                                        OnReceive?.Invoke(this, head.DataType, dataBuffer.AsSpan(0, head.DataLength));
                                        ArrayPool<byte>.Shared.Return(dataBuffer);
                                    });
                                    postDataThread.IsBackground = true;
                                    postDataThread.Start();
                                }
                                else
                                {
                                    OnReceive?.Invoke(this, head.DataType, tempDataBuffer);
                                    ArrayPool<byte>.Shared.Return(dataBuffer);
                                }
                            }
                            else if (head.TypeFlag==PackageHeadType.Leave)
                            {
                                if (head.DataLength>0)
                                {
                                    byte[] dataBuffer = ArrayPool<byte>.Shared.Rent(head.DataLength);
                                    m_tcpClient.Client.ReceiveALL(dataBuffer);
                                    Thread postDataThread = new Thread(() =>
                                    {
                                        OnReceive?.Invoke(this, head.DataType, dataBuffer.AsSpan(0, head.DataLength));
                                        ArrayPool<byte>.Shared.Return(dataBuffer);
                                    });
                                    postDataThread.IsBackground = true;
                                    postDataThread.Start();
                                }
                                else
                                {
                                    Thread postDataThread = new Thread(() =>
                                    {
                                        OnLeave?.Invoke(this, head.DataType, Span<byte>.Empty);
                                    });
                                    postDataThread.IsBackground = true;
                                    postDataThread.Start();
                                    
                                }
                                StopReceive();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            receiveThread.IsBackground=true;
            receiveThread.Start();

            return m_isStartReceive;
        }
       

        public void StopReceive()
        {
            lock (m_lockStartReceive)
            {
                m_isStartReceive=false;
            }
        }
        public void Dispose()
        {
            m_isStartReceive=false;
            m_tcpClient?.Close();
            m_tcpClient?.Dispose();
        }

    }
}
