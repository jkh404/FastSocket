using System;
using MemoryPack;

namespace FastSocket.Tcp.Package
{
    public enum PackageDataType : int
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// 字节数组
        /// </summary>
        ByteArray,
        /// <summary>
        /// 字符串
        /// </summary>
        String,
        /// <summary>
        /// Json字符串
        /// </summary>
        Json,
        /// <summary>
        /// MemoryPack系列化数据
        /// </summary>
        MemoryPack,
        /// <summary>
        /// MessagePack系列化数据
        /// </summary>
        MessagePack,
        /// <summary>
        /// PackageBody对象
        /// </summary>
        PackageBody,
        /// <summary>
        /// 分块数据包的包头
        /// </summary>
        ChunkDataHead,
        /// <summary>
        /// 分块数据包的包头
        /// </summary>
        ChunkDataBody,
    }
}
