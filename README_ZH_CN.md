# FastSocket
[English](README.md)|[中文](README_ZH_CN.md)
FastSocket 是一个基于Tcp和Udp进行通信的通信库，如名字一样，目标是极速，稳定，易用的通信库。
注意：如果您正在寻找纯粹的Socket库请使用`System.Net.Sockets`，而不是`FastSocket`

### 安装
```
dotnet add package FastSocket
```
### Tcp Server
```
using FastSocket.Tcp;
using FastSocket.Tcp.Package;
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
    channel.StartReceive();//开始接收数据
};
server.Start();
string msg;
while (!string.IsNullOrEmpty(msg = Console.ReadLine())) 
{
    server.Send(msg);
}
server.Stop();
```
### Tcp Client
```
using FastSocket.Tcp;
using FastSocket.Tcp.Package;
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
```


