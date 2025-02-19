using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FastSocket.Tcp.Package;


namespace FastSocket.Tcp
{
    public class FastTcpServer : ITcpServer
    {
        private IServerEventHandler.CheckClientHandler? m_OnCheckClient;
        private IServerEventHandler.ClientEnterEventHandler? m_OnTcpClientEnter;
        private IServerEventHandler.ClientLeaveEventHandler? m_OnTcpClientLeave;
        private IServerEventHandler.ReceiveEventHandler? m_OnServerReceive;
        public  IServerEventHandler.CheckClientHandler? OnCheckClient { get => m_OnCheckClient; set {
                if (m_isStart) throw new InvalidOperationException("TcpServer is Start");
                else m_OnCheckClient=value;
            } }
        public  IServerEventHandler.ClientEnterEventHandler? OnTcpClientEnter { get => m_OnTcpClientEnter; set {
                if (m_isStart) throw new InvalidOperationException("TcpServer is Start");
                else m_OnTcpClientEnter=value;
            } }
        public  IServerEventHandler.ClientLeaveEventHandler? OnTcpClientLeave { get => m_OnTcpClientLeave; set {
                if (m_isStart) throw new InvalidOperationException("TcpServer is Start");
                else m_OnTcpClientLeave=value;
            } }
        public  IServerEventHandler.ReceiveEventHandler? OnServerReceive { get => m_OnServerReceive; set {
                if (m_isStart) throw new InvalidOperationException("TcpServer is Start");
                else m_OnServerReceive=value;
            } }



        private  readonly TcpListener m_tcpListener;
        private readonly SingleThreadTimer m_checkTcpChannelTimer;
        private bool m_isStart;
        private object m_lockStartObj=new object();

        public bool IsStart => m_isStart;
        public FastTcpServer(IPEndPoint iPEndPoint)
        {
            m_tcpListener=new TcpListener(iPEndPoint);
            //SingleThreadTimer
            m_checkTcpChannelTimer=new SingleThreadTimer(CheckTcpChannel, 1000,1000);
        }

        private void CheckTcpChannel(object? state)
        {
            
            if (!m_tcpChannel.IsEmpty && m_tcpChannel.Count>0)
            {
                List<Guid> removeList = new List<Guid>(m_tcpChannel.Count);
                foreach ((var flag,var tcpChannel) in m_tcpChannel)
                {
                    if(!tcpChannel.Connected) removeList.Add(flag);

                }
                foreach (var flag in removeList) 
                {
                    if(m_tcpChannel.TryRemove(flag,out var removeTcpChannel))
                    {
                        m_OnTcpClientLeave?.Invoke(removeTcpChannel, PackageDataType.Unknown, Span<byte>.Empty);
                        removeTcpChannel?.OnLeave?.Invoke(removeTcpChannel, PackageDataType.Unknown, Span<byte>.Empty);
                    }
                }
            }
        }

        private ConcurrentDictionary<Guid, ITcpChannel> m_tcpChannel=new ConcurrentDictionary<Guid, ITcpChannel>();

        public void Start(bool asyncReceive = false)
        {
            if (m_isStart) throw new TcpServerIsStartException();
            lock (m_lockStartObj)
            {
                if (m_isStart) throw new TcpServerIsStartException();
                m_isStart=true;
            }
            Task.Run(() => {
                if (!m_isStart) return;
                m_tcpListener.Start();
                m_checkTcpChannelTimer?.Start();

                while (m_isStart)
                {
                    
                    var acceptThread = new Thread((obj) =>
                    {
                        TcpClient client = (obj as TcpClient)!;
                        if (client==null) return;
                        ITcpChannel tcpChannel = null;
                        try
                        {
                            
                            Span<byte> buffer = new byte[FastSocketGlobalConfiguration.PackageHeadSize];
                            client.Client.ReceiveALL(buffer);
                            var head = PackageHeadUtil.ToPackageHead(buffer);
                            if (head.FastSocketFlag==FastSocketGlobalConfiguration.FastSocketFlag && head.TypeFlag==PackageHeadType.Hello)
                            {
                                tcpChannel = new DefaultTcpChannel(client, head.ChannelFlag);
                                SayHello(tcpChannel);

                                var onLeaveAction = tcpChannel.OnLeave;
                                tcpChannel.OnLeave=(IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataBuffer) =>
                                {
                                    if (m_tcpChannel?.Remove(self.ChannelFlag, out var removeTcpChannel)??false)
                                    {
                                        m_OnTcpClientLeave?.Invoke(self, dataType, dataBuffer);
                                        
                                    }
                                    onLeaveAction?.Invoke(self, dataType, dataBuffer);
                                };
                                var onReceiveAction = tcpChannel.OnReceive;
                                tcpChannel.OnReceive=(IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
                                {
                                    m_OnServerReceive?.Invoke(self, dataType, dataPacketData);
                                    onReceiveAction?.Invoke(self, dataType, dataPacketData);
                                };
                                if (m_OnCheckClient?.Invoke(tcpChannel)??true)
                                {
                                    if (m_OnTcpClientEnter!=null)
                                    {
                                        m_tcpChannel.TryAdd(head.ChannelFlag, tcpChannel);
                                        m_OnTcpClientEnter?.Invoke(tcpChannel);
                                    }
                                    else
                                    {
                                        m_tcpChannel.TryAdd(head.ChannelFlag, tcpChannel);
                                        tcpChannel.StartReceive();
                                    }

                                }
                                else
                                {
                                    Refuse(tcpChannel);
                                    tcpChannel.StopReceive();
                                    tcpChannel.Dispose();
                                }

                            }
                            else
                            {
                                client?.Close();
                                client?.Dispose();
                                tcpChannel?.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            client?.Close();
                            client?.Dispose();
                            tcpChannel?.Dispose();
                        }
                        
                    });
                    acceptThread.IsBackground = true;
                    acceptThread.Start(m_tcpListener.AcceptTcpClient());

                }
                
                
            });
        }

        public void Stop()
        {
            if (!m_isStart) return;
            lock (m_lockStartObj)
            {
                m_isStart=false;
                foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
                {
                    ServerStop(tcpChannel);
                    tcpChannel?.StopReceive();
                    tcpChannel?.Dispose();
                }
                m_tcpListener.Stop();
                m_tcpChannel?.Clear();
                m_checkTcpChannelTimer?.Stop();
            }
        }


        public IBaseWriteOnlyTcp Send(ReadOnlySpan<byte> bytes, PackageDataType DataType = PackageDataType.ByteArray)
        {
            if (m_isStart)
            {
                
                foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
                {
                    tcpChannel?.Send(bytes, DataType);
                }
                
                return this;
            }
            else
            {
                return this;
            }
            
        }
        public async Task SendAsync(byte[] bytes, int start, int length, PackageDataType DataType = PackageDataType.ByteArray)
        {
            if (m_isStart)
            {

                List<Task> tasks = new List<Task>();
                foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        tcpChannel?.Send(bytes.AsSpan(start, length), DataType);
                    }));
                }
                await Task.WhenAll(tasks);
            }

        }

        public IBaseWriteOnlyTcp SendALLByNative(ReadOnlySpan<byte> bytes)
        {
            if (m_isStart)
            {
                foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
                {
                    tcpChannel?.SendALLByNative(bytes);
                }
                return this;
            }
            else
            {
                return this;
            }

        }
        public async Task SendALLByNativeAsync(byte[] bytes,int start,int length)
        {
            if (m_isStart)
            {
                List<Task> tasks = new List<Task>();
                foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        tcpChannel?.SendALLByNative(bytes.AsSpan(start, length));
                    }));
                }
                await Task.WhenAll(tasks);
            }

        }
        public void Dispose()
        {
            m_isStart=false;
            this.m_tcpListener.Server.Dispose();
            foreach ((Guid key, ITcpChannel tcpChannel) in m_tcpChannel)
            {
                tcpChannel?.Dispose();
            }

        }
        private static void SayHello(ITcpChannel tcpChannel)
        {
            PackageHead packageHead = new PackageHead
            {
                FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                TypeFlag=PackageHeadType.Hello,
                ChannelFlag=tcpChannel.ChannelFlag,
            };
            tcpChannel.SendALLByNative(packageHead.AsBytes());
        }
        private static void Refuse(ITcpChannel tcpChannel)
        {
            PackageHead packageHead = new PackageHead
            {
                FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                TypeFlag=PackageHeadType.Refuse,
                ChannelFlag=tcpChannel.ChannelFlag,
            };
            tcpChannel.SendALLByNative(packageHead.AsBytes());
        }

        private static void ServerStop(ITcpChannel tcpChannel)
        {
            PackageHead packageHead = new PackageHead
            {
                FastSocketFlag=FastSocketGlobalConfiguration.FastSocketFlag,
                TypeFlag=PackageHeadType.Leave,
                ChannelFlag=tcpChannel.ChannelFlag,
            };
            tcpChannel.SendALLByNative(packageHead.AsBytes());
        }

       public void WaitStop(CancellationToken cancellationToken=default)
       {
            while (m_isStart && !cancellationToken.IsCancellationRequested) 
            {
                Thread.Sleep(200);
            }
       }
    }
}
