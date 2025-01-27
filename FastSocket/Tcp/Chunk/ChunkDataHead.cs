using System;
using MemoryPack;

namespace FastSocket.Tcp
{

    [MemoryPackable]
    public partial class ChunkDataHead
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid Guid { get;  }
        /// <summary>
        /// 数据名
        /// </summary>
        public string Name { get;  }
        /// <summary>
        /// 约定的数据类型枚举
        /// </summary>
        public int DataTypeCode { get;}
        /// <summary>
        /// 使用密钥
        /// </summary>
        public bool UseKey { get;  }
        /// <summary>
        /// 分块数量 ，-1:表示分块数量未知
        /// </summary>
        public int BodyCount { get;  }
        /// <summary>
        /// 总字节长度 ，-1:表示总字节长度未知
        /// </summary>
        public long ByteLength { get; }

        public ChunkDataHead(Guid guid, string name, int dataTypeCode, bool useKey, int bodyCount, long byteLength)
        {

            Guid=guid;
            Name=name;
            DataTypeCode=dataTypeCode;
            UseKey=useKey;
            BodyCount=bodyCount;
            ByteLength=byteLength;
        }
    }
}
