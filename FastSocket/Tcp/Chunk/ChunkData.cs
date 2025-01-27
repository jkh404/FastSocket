using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace FastSocket.Tcp.Chunk
{
    public class ChunkData : IChunkData, IDisposable
    //:IEquatable<ChunkData>
    {
        public Guid Guid => m_dataHead!.Guid;
        public ChunkDataHead Head => m_dataHead!;
        private  ChunkDataHead? m_dataHead;
        private  Action<AppendResult>? m_appendAction;
        private  Action<Stream>? m_completedAction;
        private Stream? m_Stream = null;

        private AppendResult m_AppendResult;

        public bool IsCompleted { get; private set; } = false;
        //private int m_BodyCount = 0;
        private ChunkData() { m_AppendResult=null; }
        internal ChunkData(ChunkDataHead dataHead, Action<AppendResult>? appendAction, Action<Stream>? completedAction)
        {
            m_dataHead=dataHead;
            m_appendAction=appendAction;
            m_completedAction=completedAction;
            m_AppendResult=new AppendResult(m_dataHead.BodyCount, m_dataHead.ByteLength);

        }
        public void SetStream(Stream stream)
        {
            if (m_Stream!=null) throw new InvalidOperationException();
            m_Stream=stream;
        }
        public void Append(ChunkDataBody chunkDataBody)
        {

            if (m_Stream!=null)
            {
                
                var data = chunkDataBody.Data;
                var byteLen = data.Length;
                var byteStart = chunkDataBody.Start;
                if (m_dataHead.UseKey)
                {
                    //解密数据

                }

                if (byteStart>=0)//支持分块写入
                {
                    m_Stream?.Seek(byteStart, SeekOrigin.Begin);
                    m_Stream?.Write(data);
                    m_AppendResult.ThisTimeReceivedByte=data.Length;
                    m_AppendResult.WaitReceivedBody--;
                    m_AppendResult.WaitReceivedByte-=byteLen;
                    m_appendAction?.Invoke(m_AppendResult);
                    if (m_AppendResult.WaitReceivedByte==0)
                    {
                        IsCompleted=true;
                        m_completedAction?.Invoke(m_Stream);
                    }
                    m_AppendResult.LastTick=DateTime.Now.Ticks;
                    return;
                }
                else if (byteStart==-1)//结束
                {
                    IsCompleted=true;
                    m_completedAction?.Invoke(m_Stream);
                }

              

                
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            
            m_Stream?.Flush();
            m_Stream?.Close();
            m_Stream?.Dispose();
            m_Stream=null;
            m_appendAction=null;
            m_AppendResult=null;
            m_completedAction=null;
            m_dataHead=null;
        }
        public record class AppendResult
        {
            /// <summary>
            /// 分块数量
            /// </summary>
            public long BodyCount { get; }
            /// <summary>
            /// 总字节长度
            /// </summary>
            public long ByteLength { get; }

            public long ThisTimeReceivedByte { get; internal set; }
            public long WaitReceivedBody { get; internal set; }
            public long WaitReceivedByte { get; internal set; }

            public long? LastTick { get; internal set; }
            public AppendResult(long bodyCount, long byteLength)
            {
                BodyCount=bodyCount;
                ByteLength=byteLength;
                WaitReceivedBody=bodyCount;
                WaitReceivedByte=byteLength;
            }
        }

    }
}
