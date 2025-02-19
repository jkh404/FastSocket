using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Channels;
using FastSocket.Tcp;
using FastSocket.Tcp.Package;
using MemoryPack;


FastTcpServer server = new FastTcpServer(new IPEndPoint(IPAddress.Loopback, 9090));
server.OnTcpClientEnter=(channel) =>
{
    Console.WriteLine("[客户端进入]");
    channel.OnReceive=(self, dataType, data) =>
    {
        if (dataType==PackageDataType.String)
        {
            var msg = Encoding.UTF8.GetString(data);
            Console.WriteLine(value: $"[C:{msg}]");
        }
    };

    channel.StartReceive();
};
server.Start();
string msg;
while (!string.IsNullOrEmpty(msg = Console.ReadLine()))
{
    server.Send(msg);

}
server.Stop();
return;




//FastTcpServer fastTcpServer = new FastTcpServer(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 9090));
//fastTcpServer.OnCheckClient=(IBaseTcpClient self) => true;
//fastTcpServer.OnTcpClientEnter=(ITcpChannel c) =>
//{
//    //c.OnReceive=static (IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) => {
//    //    if (dataType==PackageDataType.ChunkDataHead)
//    //    {
//    //        var head=dataPacketData.ToChunkDataHead();
//    //        if (head!=null)
//    //        {
//    //            self.AwaitChunk(head, initAction: (chunkData) => {
//    //                chunkData.SetStream(File.OpenWrite($"./{head.Name}"));
//    //                Console.WriteLine($"开始接收 {head.Name}");
//    //            }, appendAction: (result) =>
//    //            {
//    //                Console.WriteLine($"接收到字节：{result.ThisTimeReceivedByte},剩余字节：{result.WaitReceivedByte}");
//    //            }, completedAction: (stream) =>
//    //            {
//    //                Console.WriteLine($"{head.Name} 接收完毕");
//    //            });
//    //        }
//    //    }else if (dataType==PackageDataType.ChunkDataBody)
//    //    {
//    //        var body=dataPacketData.ToChunkDataBody();
//    //        self.AppendChunk(body);
//    //    }


//    //};
//    //c.OnLeave=(IBaseTcpChannel self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
//    //{
//    //    self.ClearChunk();
//    //};
//    c.StartReceive();
//    const string fileName = "test2.png";
//    using Stream stream = File.OpenRead($"./{fileName}");
//    for (int i = 0; i < 10; i++)
//    {
//        stream.Seek(0, SeekOrigin.Begin);
//        c.SendChunk(stream, fileName, oneChunkSize: 1024);
//    }
//    c.ClearChunk();
//};

//fastTcpServer.Start();
//fastTcpServer.WaitStop();

















/********************************测速代码***********************************************************/
ConcurrentBag<double> byteLengths = new ConcurrentBag<double>();
StringBuilder stringBuilder = new StringBuilder();
for (int i = 0; i < 1 * 1024; i++)
{
    stringBuilder.Append($"{i % 10}");
}
string msgText = stringBuilder.ToString();
byte[] msgData = Encoding.UTF8.GetBytes(msgText);

FastTcpServer fastTcpServer = new FastTcpServer(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 9090));
fastTcpServer.OnTcpClientEnter = (ITcpChannel client) =>
{
    
    client.StartReceive(asyncReceive: false);
    Console.WriteLine($"客户端进入:{client.ChannelFlag}");
    
};
fastTcpServer.OnTcpClientLeave = (IBaseTcpChannel client, PackageDataType dataType, ReadOnlySpan<byte> data) =>
{
    Console.WriteLine($"客户端离开:{client.ChannelFlag}");
};
fastTcpServer.OnServerReceive = (IBaseTcpChannel client, PackageDataType dataType, ReadOnlySpan<byte> data) =>
{
    if (dataType == PackageDataType.PackageBody)
    {
        var text = MemoryPackSerializer.Deserialize<PackageBody>(data).ToObj<string>();
        byteLengths.Add((text?.Length ?? 0) * 2 / 1024);
    }
    //byteLengths.Add(data.Length/1024);
    //Console.WriteLine($"客户端离开:{client.ChannelFlag}");
};
fastTcpServer.Start(asyncReceive: true);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(async () =>
{

    while (true)
    {
        await fastTcpServer.SendAsync(msgData, 0, msgData.Length);
        Thread.Sleep(100);
    }
});
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
Task.Run(() =>
{
    DateTime dateTime = DateTime.Now;
    while (true)
    {
        while ((DateTime.Now - dateTime).TotalMilliseconds < 990)
        {
            Thread.Sleep(100);
        }
        var speed = byteLengths.Sum() / 1204 / (DateTime.Now - dateTime).TotalSeconds;
        byteLengths.Clear();
        dateTime = DateTime.Now;
        Console.WriteLine($"速度：{speed:N2} MB/s");
    }
});
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法


Console.ReadLine();
fastTcpServer.Stop();
/*******************************************************************************************/


//SingleThreadTimer singleThreadTimer = new SingleThreadTimer(callback, 1000, 1000);
//singleThreadTimer.Start();
//Console.ReadLine();
//void callback(object? state)
//{
//    Console.WriteLine(DateTime.Now.TimeOfDay);
//}


//DiscoverDeviceService discoverDeviceService = new DiscoverDeviceService();
//discoverDeviceService.Start(new DeviceInfo
//{
//    Name="A",
//}.NewGuid(),
//onJoin: (devc) =>
//{
//    Console.WriteLine($"【{devc.Name}】在线");
//},
//onLeave: (devc) =>
//{
//    Console.WriteLine($"【{devc.Name}】离线");
//}
//);


//while (Console.ReadLine()!="")
//{

//}