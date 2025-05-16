using System;
using MemoryPack;


namespace FastSocket.Tcp
{
    [MemoryPackable]
    public partial class PackageBody
    {
        public string TypeFullNname { get; set; }
        public byte[] Data {  get; set; }

        public T? ToObj<T>()
        {
            if (typeof(T).FullName== TypeFullNname)
            {
                
                return MemoryPackSerializer.Deserialize<T>(Data);
            }
            else
            {
                return default(T?);
            }
        }
        public object? GetObj()
        {
            var type = Type.GetType(TypeFullNname);
            if (type!=null) return MemoryPackSerializer.Deserialize(type, Data);
            else return null;
        }
    }
}
