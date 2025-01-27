using System;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{
    public interface IServerEventHandler
    {
        /// <summary>
        /// 客户端进入对其进行配置检查
        /// </summary>
        /// <param name="tcpChannel">客户端</param>
        /// <returns>是否允许连接</returns>
        public delegate bool CheckClientHandler(IBaseTcpClient tcpChannel);
        /// <summary>
        /// 客户端完成验证并进入事件
        /// </summary>
        /// <param name="tcpChannel">客户端通道</param>
        public delegate void ClientEnterEventHandler(ITcpChannel tcpChannel);
        /// <summary>
        /// 客户端离开
        /// </summary>
        /// <param name="baseTcpChannel">客户端</param>
        /// <param name="dataType">离开时最后发送的数据的类型</param>
        /// <param name="bytes">离开时最后发送的数据</param>
        public delegate void ClientLeaveEventHandler(IBaseTcpChannel baseTcpChannel, PackageDataType dataType, ReadOnlySpan<byte> bytes);
        /// <summary>
        /// 客户端数据到达
        /// </summary>
        /// <param name="baseTcpChannel">客户端</param>
        /// <param name="dataType">数据的类型</param>
        /// <param name="dataPacketData">数据</param>
        public delegate void ReceiveEventHandler(IBaseTcpChannel baseTcpChannel, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData);
        public CheckClientHandler? OnCheckClient {  get; set;  }
        public   ClientEnterEventHandler? OnTcpClientEnter {  get; set;  }
        public ClientLeaveEventHandler? OnTcpClientLeave {  get; set;  }
        public ReceiveEventHandler? OnServerReceive {  get; set;  }
    }
}
