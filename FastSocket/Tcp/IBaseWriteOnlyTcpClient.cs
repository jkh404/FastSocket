using System;
using System.Net;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{
    public interface IBaseWriteOnlyTcpClient : IBaseTcpClient,IBaseWriteOnlyTcp
    {
        //public TcpClientOption ClientOption { get; }
        //public int Send(string msg);
        //public int Send(byte[] buffer);


    }
}
