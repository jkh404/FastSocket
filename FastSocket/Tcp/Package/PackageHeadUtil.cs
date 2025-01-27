using System;
using System.Runtime.CompilerServices;
using FastSocket.Tcp.Package;


namespace FastSocket.Tcp
{
    public static class PackageHeadUtil
    {
        public unsafe static Span<byte> AsBytes(this ref PackageHead head)
        {
            return new Span<byte>(Unsafe.AsPointer(ref head), FastSocketGlobalConfiguration.PackageHeadSize);
        }
        public unsafe static PackageHead ToPackageHead(Span<byte> bytes)
        {
            if (bytes.Length==sizeof(PackageHead))
            {
                return Unsafe.AsRef<PackageHead>(Unsafe.AsPointer(ref bytes[0]));
            }
            else
            {
                throw new ArgumentException();

            }

        }
    }
}
