using System;
using System.Threading.Tasks;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{

    public interface ITcpServer : IServerEventHandler,IBaseWriteOnlyTcp, IDisposable
    {
        public bool IsStart { get; }
        
        public Task SendAsync(byte[] bytes, int start, int length, PackageDataType packageType = PackageDataType.ByteArray);
        
        public Task SendALLByNativeAsync(byte[] bytes, int start, int length);

        public void Start(bool asyncReceive = false);
        public void Stop();
    }
}
