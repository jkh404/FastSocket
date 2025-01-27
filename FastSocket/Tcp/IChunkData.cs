using System;
using System.IO;

namespace FastSocket.Tcp
{
    public interface IChunkData
    {
        public Guid Guid { get; }
        public ChunkDataHead Head { get; }
        public bool IsCompleted { get;  }
        public void SetStream(Stream stream);
    }
}
