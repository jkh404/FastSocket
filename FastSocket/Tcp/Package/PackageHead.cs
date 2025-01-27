using System;
using System.Runtime.InteropServices;
using MemoryPack;


namespace FastSocket.Tcp.Package
{
    [StructLayout(LayoutKind.Explicit, Size = FastSocketGlobalConfiguration.PackageHeadSize)]
    public struct PackageHead
    {
        [FieldOffset(0)] public int FastSocketFlag;
        [FieldOffset(4)] public PackageHeadType TypeFlag;
        [FieldOffset(8)] public Guid ChannelFlag;
        [FieldOffset(24)] public int DataLength;
        [FieldOffset(28)] public PackageDataType DataType;
    }
}
