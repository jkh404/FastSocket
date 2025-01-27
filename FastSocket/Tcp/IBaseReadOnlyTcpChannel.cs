namespace FastSocket.Tcp
{
    public interface IBaseReadOnlyTcpChannel : IBaseTcpClient
    {
        public bool IsStartReceive { get; }
        public bool StartReceive(bool asyncReceive=false);
        public void StopReceive();
    }
}
