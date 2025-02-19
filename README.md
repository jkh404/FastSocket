# FastSocket
[English](README.md)|[中文](README_ZH_CN.md)
FastSocket is a communication library based on Tcp and Udp for networking. As its name suggests, it aims to be extremely fast, stable, and easy to use. Note: If you are looking for a pure Socket library, please use `System.Net.Sockets` instead of `FastSocket`.
### Installation
```
dotnet add package Sky.FastSocket
```
### Tcp Server
```
using FastSocket.Tcp;
using FastSocket.Tcp.Package;
FastTcpServer server = new FastTcpServer(new IPEndPoint(IPAddress.Loopback, 9090));
server.OnTcpClientEnter=(channel) =>
{
    Console.WriteLine("[ClientEnter]");
    channel.OnReceive=(self, dataType, data) =>
    {
        if (dataType==PackageDataType.String)
        {
            var msg = Encoding.UTF8.GetString(data);
            Console.WriteLine(value: $"[C:{msg}]");
        }
    };
    channel.StartReceive();//Start receiving data
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
client.OnReceive=(self, dataType, data) =>//Receive data
{
    if (dataType==PackageDataType.String)
    {
        var msg=Encoding.UTF8.GetString(data);
        Console.WriteLine($"[S:{msg}]");
    }
};
client.OnConnected=() => Console.WriteLine("[Connected]");
client.Connect("localhost",9090);
string msg;
while (!string.IsNullOrEmpty(msg = Console.ReadLine())) 
{
    client.Send(msg);
}
```


