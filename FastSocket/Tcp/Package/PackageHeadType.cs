namespace FastSocket.Tcp.Package
{
    public enum PackageHeadType : int
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// 初次连接
        /// </summary>
        Hello,
        /// <summary>
        /// 数据到达
        /// </summary>
        Data,
        /// <summary>
        /// 离线消息
        /// </summary>
        Leave,
        /// <summary>
        /// 拒绝
        /// </summary>
        Refuse,
    }
}
