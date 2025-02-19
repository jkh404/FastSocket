using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using FastSocket;
using FastSocket.Tcp;
using FastSocket.Tcp.Package;
using static System.Runtime.InteropServices.JavaScript.JSType;

FastTcpClient client = new FastTcpClient();
client.OnReceive=(self, dataType, data) =>//接收到数据
{
    if (dataType==PackageDataType.String)
    {
        var msg=Encoding.UTF8.GetString(data);
        Console.WriteLine($"[S:{msg}]");
    }
    
};
client.OnConnected=() => Console.WriteLine("[连接成功]");
client.Connect("localhost",9090);
string msg;
while (!string.IsNullOrEmpty(msg = Console.ReadLine()))
{
    client.Send(msg);
}
return;



//SingleThreadTimer threadTimer = new SingleThreadTimer((state) =>
//{
//    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}");
//},100,1000);
//threadTimer.Start();
//Console.ReadLine();
//threadTimer.Stop();
//return;
FastTcpClient fastTcpClient = new FastTcpClient();

fastTcpClient.OnReceive=(IBaseTcpClient self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
{
    if (dataType==PackageDataType.ChunkDataHead)
    {
        var head = dataPacketData.ToChunkDataHead();
        if (head!=null)
        {
            self.AwaitChunk(head, initAction: (chunkData) => {
                chunkData.SetStream(File.OpenWrite($"./{head.Name}"));
                Console.WriteLine($"开始接收 {head.Name}");
            }, appendAction: (result) =>
            {
                Console.WriteLine($"接收到字节：{result.ThisTimeReceivedByte},剩余字节：{result.WaitReceivedByte}");
            }, completedAction: (stream) =>
            {
                Console.WriteLine($"{head.Name} 接收完毕");
            });
        }
    }
    else if (dataType==PackageDataType.ChunkDataBody)
    {
        var body = dataPacketData.ToChunkDataBody();
        self.AppendChunk(body);
    }
};
fastTcpClient.OnServerStop=(IBaseTcpClient self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
{
    self.ClearChunk();
};
fastTcpClient.Connect("127.0.0.1", 9090);
if (fastTcpClient.Connected)
{
    //const string fileName = "test.png";
    //using Stream stream=File.OpenRead($"./{fileName}");
    //for (int i = 0; i < 1000000; i++)
    //{
    //    stream.Seek(0, SeekOrigin.Begin);
    //    fastTcpClient.SendChunk(stream, fileName, oneChunkSize: 1024);
    //}
    
}
fastTcpClient.WaitStop();






/********************************测速代码***********************************************************/
//int ClientCount = 1000;
//ConcurrentBag<double> byteLengths = new ConcurrentBag<double>();
//StringBuilder stringBuilder = new StringBuilder();
//for (int i = 0; i < 1*1024; i++)
//{
//    stringBuilder.Append($"{i%10}");
//}
//string msgText = stringBuilder.ToString();
//byte[] msgData = Encoding.UTF8.GetBytes(msgText);
//List<FastTcpClient> list=new List<FastTcpClient>(ClientCount);
//for (int i = 0; i<ClientCount; i++)
//{
//    FastTcpClient tcpClient = new FastTcpClient();
//    tcpClient.OnReceive=(IBaseTcpClient self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
//    {

//        //if (dataType==PackageDataType.String)
//        //{
//        //    byteLengths.Add(msgData.Length/1024);
//        //    Console.WriteLine($"收到:{Encoding.UTF8.GetString(dataPacketData)}");
//        //}
//        byteLengths.Add(msgData.Length/1024);
//    };
//    tcpClient.OnServerStop=(IBaseTcpClient self, PackageDataType dataType, ReadOnlySpan<byte> dataPacketData) =>
//    {
//        Console.WriteLine("服务端停止");
//    };
//    tcpClient.Connect("127.0.0.1", 9090);
//    list.Add(tcpClient);
//    Console.WriteLine($"[{tcpClient.ChannelFlag}]已连接");
//}

////new SingleThreadTimer((obj) =>
////{
////    var speed = byteLengths.Sum()/1204;
////    byteLengths.Clear();
////    Console.WriteLine($"速度：{speed:N2} MB/s");
////}, 1000, 1000).Start();

////foreach (var item in list)
////{
////    var thread=new Thread(() =>
////    {
////        Thread.Sleep(100);
////        while (true)
////        {
////            Thread.Sleep(100);
////            item.SendPackge(text);
////            byteLengths.Add(msgData.Length/1024);

////        }
////    });
////    thread.IsBackground = true;
////    thread.Start();
////}
//var thread2 = new Thread(() =>
//{
//    DateTime dateTime = DateTime.Now;
//    while (true)
//    {
//        Thread.Sleep(1000);
//        var speed = byteLengths.Sum()/1204/(DateTime.Now-dateTime).TotalSeconds;
//        dateTime = DateTime.Now;
//        byteLengths.Clear();
//        Console.WriteLine($"速度：{speed:N2} MB/s");
//    }
//});
//thread2.IsBackground = true;
//thread2.Start();

//Console.ReadLine();
/******************************************************************************************/

//DiscoverDeviceService discoverDeviceService = new DiscoverDeviceService();
//discoverDeviceService.Start(new DeviceInfo
//{
//    Name="B",
//}.NewGuid(),
//onJoin: (devc) =>
//{
//    Console.WriteLine($"{devc.Name}加入");
//},
//onLeave: (devc) =>
//{
//    Console.WriteLine($"{devc.Name}可能掉线");
//}
//);


//while (Console.ReadLine()!="")
//{

//}