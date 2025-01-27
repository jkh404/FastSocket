using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FastSocket.Tcp.Chunk;
using FastSocket.Tcp.Package;
using MemoryPack;

namespace FastSocket.Tcp
{

    public static class ChunkDataUtil
    {
        public static ChunkDataHead? ToChunkDataHead(this ReadOnlySpan<byte> bytes)
        {
            return MemoryPackSerializer.Deserialize<ChunkDataHead>(bytes);
        }
        public static ChunkDataBody ToChunkDataBody(this ReadOnlySpan<byte> bytes)
        {
            Guid guid = new Guid(bytes.Slice(0,16));
            long start= bytes.Slice(16, 8).ToLong();
            ChunkDataBody chunkDataBody=new ChunkDataBody(guid, start, bytes[24..]);
            return chunkDataBody;
        }
        private static ConcurrentDictionary<Guid, Dictionary<Guid, ChunkData>> m_Dic = new ConcurrentDictionary<Guid, Dictionary<Guid, ChunkData>> ();
        public static void AwaitChunk(this IBaseTcpClient main , ChunkDataHead chunkDataHead, Action<IChunkData> initAction, Action<ChunkData.AppendResult>? appendAction = null, Action<Stream>? completedAction = null)
        {
            if(!m_Dic.ContainsKey(main.ChannelFlag)) m_Dic[main.ChannelFlag]=new Dictionary<Guid, ChunkData> ();
            var dic = m_Dic[main.ChannelFlag];
            if (dic.TryAdd(chunkDataHead.Guid, new ChunkData(chunkDataHead, appendAction, (stream) =>
            {
                dic.Remove(chunkDataHead.Guid,out var removeChunkData);
                completedAction?.Invoke(stream);
                removeChunkData?.Dispose();
            })))
            {
                initAction?.Invoke(dic[chunkDataHead.Guid]);
            }
            else 
            {
                throw new InvalidOperationException();
            }

        }
        public static bool AppendChunk(this IBaseTcpClient main, ChunkDataBody chunkDataBody)
        {
            if (m_Dic.ContainsKey(main.ChannelFlag))
            {
                var dic = m_Dic[main.ChannelFlag];
                var id = chunkDataBody.Guid;
                var chunkData = dic[id];
                if (chunkData==null) return false;
                chunkData?.Append(chunkDataBody);
                return true;
            }else { return false; }
                
        }
        public static void ClearChunk(this IBaseTcpClient main)
        {
            m_Dic.TryRemove(main.ChannelFlag, out var dic);
            if (dic!=null)
            {
                foreach (var item in dic)
                {
                    item.Value?.Dispose();
                }
                dic.Clear();
            }
        }

        public static bool SendChunk(this IBaseWriteOnlyTcp main,Stream stream,string chunkName="", int dataTypeCode=0,bool useKey=false, int oneChunkSize=1024*1024*10)
        {
            if (stream.CanRead)
            {
                long byteLen = 0;
                try
                {
                    byteLen = stream.Length;
                }
                catch
                {
                    byteLen=-1;
                }
                
                int bodyCount = ((int)(byteLen/oneChunkSize));
                if (byteLen%oneChunkSize>0) bodyCount+=1;

                if (byteLen<0) bodyCount=-1;

                const int bodyheadSize = 16+8;

                ChunkDataHead chunkDataHead = new ChunkDataHead(Guid.NewGuid(), chunkName, dataTypeCode, useKey, bodyCount, byteLen);
                
                var buffer=ArrayPool<byte>.Shared.Rent(oneChunkSize+bodyheadSize);
                var bufferSpan = buffer.AsSpan(bodyheadSize, oneChunkSize);
                int bytesRead=0;
                long pos = -1;
                try
                {
                    pos = (stream.Position-bytesRead);
                }
                catch (Exception)
                {
                    pos=-2;//赋值为-1，表示Stream不支持读取位置
                }

                main.Send(MemoryPackSerializer.Serialize(chunkDataHead), PackageDataType.ChunkDataHead);//先发送head信息
                var headGuid= chunkDataHead.Guid;
                headGuid.TryWriteBytes(buffer.AsSpan(0, 16));
                pos.WriteTo(buffer.AsSpan(16, 8));
                while ((bytesRead=stream.Read(bufferSpan))>0)
                {
                    if(pos>=0)
                    {
                        pos= (stream.Position-bytesRead);
                        pos.WriteTo(buffer.AsSpan(16, 8));
                        main.Send(buffer.AsSpan(0, bytesRead+bodyheadSize), PackageDataType.ChunkDataBody);
                    }
                    
                    
                }
                if (pos<0)
                {
                    pos= -1;
                    pos.WriteTo(buffer.AsSpan(16, 8));
                    main.Send(buffer.AsSpan(0, bytesRead+bodyheadSize), PackageDataType.ChunkDataBody);
                }
                ArrayPool<byte>.Shared.Return(buffer);
                return true;
            }
            else
            {
                return false;
            }
        }

        
    }
}
