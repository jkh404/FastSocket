using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FastSocket.Tcp.Package;


namespace FastSocket.Tcp
{
    public class FastTcpClient : ITcpClient
    {
        private IClientEventHandler.ReceiveEventHandler? m_OnReceive;
        private IClientEventHandler.ServerStopEventHandler? m_OnServerStop;
        private IClientEventHandler.ConnectedEventHandler? m_OnConnected;
        public IClientEventHandler.ReceiveEventHandler? OnReceive { get => m_OnReceive; set {
                if (m_isStartReceive) throw new InvalidOperationException("TcpClient is Start");
                else m_OnReceive=value;
            } }
        public IClientEventHandler.ServerStopEventHandler? OnServerStop { get => m_OnServerStop; set {
                if (m_isStartReceive) throw new InvalidOperationException("TcpClient is Start");
                else m_OnServerStop=value;
            }
        }
        public IClientEventHandler.ConnectedEventHandler? OnConnected{ get => m_OnConnected; set {
                if (m_isStartReceive) throw new InvalidOperationException("TcpClient is Start");
                else m_OnConnected=value;
            }
        }
        private readonly TcpClient m_tcpClient;
        private readonly Guid m_channelFlag;
        private readonly PackageHead  m_packageHead;
        

        public bool Connected => m_isStartReceive && m_tcpClient.Client.Connected;

        public int Available => m_tcpClient.Available;

        public Guid ChannelFlag => m_channelFlag;
        public IPEndPoint LocalIPEndPoint => (m_tcpClient.Client.LocalEndPoint as IPEndPoint)!;
        public IPEndPoint RemoteIPEndPoint => (m_tcpClient.Client.RemoteEndPoint as IPEndPoint)!;

        private object m_lockStartReceive=new object();
        private object m_lockSend = new object();
        private bool m_isStartReceive;

        public FastTcpClient()
        {
            m_tcpClient=new TcpClient();
            m_channelFlag=Guid.NewGuid();
            m_packageHead=new PackageHead()
            {
                FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                ChannelFlag = ChannelFlag,
                TypeFlag=PackageHeadType.Hello,
            };
        }
        public bool Connect(string host, int port)
        {
            
            m_tcpClient.Connect(host, port);
            return Start() && m_tcpClient.Client.Connected;
        }
        public bool Connect(IPEndPoint iPEndPoint)
        {
            m_tcpClient.Connect(iPEndPoint);
            return Start() && m_tcpClient.Client.Connected;
        }
        private bool Start()
        {
            if (m_isStartReceive) return false;
            lock (m_lockStartReceive)
            {
                if (m_isStartReceive) return false;
                m_isStartReceive=true;
                SayHello();
                Span<byte> buffer = stackalloc byte[FastSocketGlobalConfiguration.PackageHeadSize];
                m_tcpClient.Client.ReceiveALL(buffer);
                var head = PackageHeadUtil.ToPackageHead(buffer);
                if (head.FastSocketFlag==FastSocketGlobalConfiguration.FastSocketFlag
                    && head.ChannelFlag==m_channelFlag)
                {
                    if (head.TypeFlag!=PackageHeadType.Hello)
                    {
                        m_isStartReceive=false;
                        return m_isStartReceive;
                    }
                    else
                    {
                        m_OnConnected?.Invoke();
                    }
                }
            }
            if (m_isStartReceive)
            {

                var receiveThread = new Thread(() =>
                {
                    Span<byte> buffer = new byte[FastSocketGlobalConfiguration.PackageHeadSize];
                    while (m_isStartReceive && m_tcpClient.Client.Connected)
                    {
                        try
                        {
                            m_tcpClient.Client.ReceiveALL(buffer);
                            var head = PackageHeadUtil.ToPackageHead(buffer);
                            if (head.FastSocketFlag==FastSocketGlobalConfiguration.FastSocketFlag
                                && head.ChannelFlag==m_channelFlag)
                            {
                                if (head.TypeFlag==PackageHeadType.Data && head.DataLength>0)
                                {
                                    ReadData(head.DataLength, (tempDataBuffer) =>
                                    {
                                        OnReceive?.Invoke(this, head.DataType, tempDataBuffer);
                                    });
                                }
                                else if (head.TypeFlag==PackageHeadType.Leave)
                                {
                                    Close();
                                    if (head.DataLength>0)
                                    {
                                        ReadData(head.DataLength, (tempDataBuffer) =>
                                        {
                                            OnServerStop?.Invoke(this, head.DataType, tempDataBuffer);
                                        });
                                    }
                                    else
                                    {
                                        OnServerStop?.Invoke(this, head.DataType, Span<byte>.Empty);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //m_OnServerStop?.Invoke(this, PackageDataType.Unknown, null);
                            Console.WriteLine(ex);
                            DisconnectNoSayBye();
                        }

                    }
                });
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            return m_isStartReceive;
        }
        private delegate void ReadDataCallBack(ReadOnlySpan<byte> bytes);
        private void ReadData(int DataLength, ReadDataCallBack readDataCallBack)
        {
            byte[] dataBuffer = ArrayPool<byte>.Shared.Rent(DataLength);
            Span<byte> tempDataBuffer = dataBuffer.AsSpan(0, DataLength);
            m_tcpClient.Client.ReceiveALL(tempDataBuffer);
            readDataCallBack?.Invoke(tempDataBuffer);
            ArrayPool<byte>.Shared.Return(dataBuffer);
        }
        private void Close()
        {
            lock (m_lockStartReceive)
            {
                m_isStartReceive=false;
            }
            m_tcpClient?.Close();
            m_tcpClient?.Dispose();
        }
        public void Disconnect()
        {
            lock (m_lockStartReceive)
            {
                SayBye();
                m_isStartReceive=false;
                
            }
            m_tcpClient?.Close();
            m_tcpClient?.Dispose();
        }
        private void DisconnectNoSayBye()
        {
            lock (m_lockStartReceive)
            {
                m_isStartReceive=false;
                
            }
            m_tcpClient?.Close();
            m_tcpClient?.Dispose();
        }
        private void SayHello()
        {
            lock (m_lockSend)
            {
                PackageHead packageHead = new PackageHead
                {
                    FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                    TypeFlag=PackageHeadType.Hello,
                    ChannelFlag=m_channelFlag,
                };
                SendALLByNative(packageHead.AsBytes());
            }
        }
        private void SayBye()
        {
            lock (m_lockSend)
            {
                PackageHead packageHead = new PackageHead
                {
                    FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                    TypeFlag=PackageHeadType.Leave,
                    ChannelFlag=m_channelFlag,
                };
                SendALLByNative(packageHead.AsBytes());
            }
        }
        public IBaseWriteOnlyTcp Send(ReadOnlySpan<byte> buffer, PackageDataType packageType = PackageDataType.ByteArray)
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
                    DataType=packageType
                };
                SendALLByNative(packageHead.AsBytes());
                SendALLByNative(buffer);
            }
            return this;
        }
        public IBaseWriteOnlyTcp SendALLByNative(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length==0) return this;
            else { m_tcpClient.Client.SendALL(buffer); return this; }
        }
        public void Dispose()
        {
            m_tcpClient?.Close();
            m_tcpClient?.Dispose();
        }
        public void WaitStop(CancellationToken cancellationToken = default)
        {
            while (m_isStartReceive && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(200);
            }
        }

    }
}
