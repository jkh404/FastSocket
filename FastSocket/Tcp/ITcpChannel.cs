using System;

namespace FastSocket.Tcp
{
    public interface IBaseTcpChannel : IBaseReadOnlyTcpChannel, IBaseWriteOnlyTcpChannel
    {

    }
    public interface ITcpChannel : IBaseTcpChannel, IChannelEventHandler, IDisposable
    {

    }
}
