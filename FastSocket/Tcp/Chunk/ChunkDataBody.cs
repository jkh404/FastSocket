using System;
using MemoryPack;

namespace FastSocket.Tcp
{

    public ref struct  ChunkDataBody
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid Guid { get; }
        /// <summary>
        /// 数据起始位置 -1:已结束，-2:起始位置未知
        /// </summary>
        public long Start { get; }
        /// <summary>
        /// 数据
        /// </summary>
        public ReadOnlySpan<byte> Data { get; }

        public ChunkDataBody(Guid guid, long start, ReadOnlySpan<byte> data)
        {
            Guid=guid;
            Start=start;
            Data=data;
        }
    }
}
