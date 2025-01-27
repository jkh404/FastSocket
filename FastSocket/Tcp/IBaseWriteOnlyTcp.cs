using System;
using FastSocket.Tcp.Package;

namespace FastSocket.Tcp
{
    public interface IBaseWriteOnlyTcp
    {
        public IBaseWriteOnlyTcp Send(ReadOnlySpan<byte> buffer, PackageDataType packageType = PackageDataType.ByteArray);
        public IBaseWriteOnlyTcp SendALLByNative(ReadOnlySpan<byte> buffer);
    }
}
