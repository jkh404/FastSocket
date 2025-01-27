using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FastSocket.Tcp.Package;
using MemoryPack;

namespace FastSocket.Tcp
{
    public static class TcpExtension
    {
        public static void Send(this IBaseTcpChannel main,string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg),PackageDataType.String);
        }
        public static void Send(this ITcpClient main, string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg), PackageDataType.String);
        }
        public static void Send(this ITcpServer main, string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg), PackageDataType.String);
        }

        public static void SendJson(this IBaseTcpChannel main, string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg), PackageDataType.Json);
        }
        public static void SendJson(this ITcpClient main, string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg), PackageDataType.Json);
        }
        public static void SendJson(this ITcpServer main, string msg)
        {
            main.Send(Encoding.UTF8.GetBytes(msg), PackageDataType.Json);
        }


        public static void SendObj<T>(this IBaseTcpChannel main, T msg)
        {
            main.Send(MemoryPackSerializer.Serialize(msg), PackageDataType.MemoryPack);
        }
        public static void SendObj<T>(this ITcpClient main, T msg)
        {
            main.Send(MemoryPackSerializer.Serialize(msg), PackageDataType.MemoryPack);
        }
        public static void SendObj<T>(this ITcpServer main, T msg)
        {
            main.Send(MemoryPackSerializer.Serialize(msg), PackageDataType.MemoryPack);
        }


        public static void SendPackge<T>(this IBaseTcpChannel main, T msg)
        {
            PackageBody body = new PackageBody();
            body.TypeFullNname=typeof(T).FullName!;
            body.Data=MemoryPackSerializer.Serialize(msg);
            main.Send(MemoryPackSerializer.Serialize(body), PackageDataType.PackageBody);
        }
        public static void SendPackge<T>(this ITcpClient main, T msg)
        {
            PackageBody body = new PackageBody();
            body.TypeFullNname=typeof(T).FullName!;
            body.Data=MemoryPackSerializer.Serialize(msg);
            main.Send(MemoryPackSerializer.Serialize(body), PackageDataType.PackageBody);
        }
        public static void SendPackge<T>(this ITcpServer main, T msg)
        {
            PackageBody body = new PackageBody();
            body.TypeFullNname=typeof(T).FullName!;
            body.Data=MemoryPackSerializer.Serialize(msg);
            main.Send(MemoryPackSerializer.Serialize(body), PackageDataType.PackageBody);
        }
    }
}
