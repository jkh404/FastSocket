using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastSocket
{
    internal static class BaseUtil
    {
        public static int ToInt(this ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 4) throw new ArgumentException(nameof(bytes));

            return (bytes[0])
                | (bytes[1] << 8)
                | (bytes[2] << 16)
                | (bytes[3] << 24);
        }
        public static void WriteTo(this int value, Span<byte> bytes)
        {
            if (bytes.Length!=4) throw new ArgumentException(nameof(bytes));
            bytes[0] = (byte)(value& 0xFF);
            bytes[1] = (byte)((value >> 8)& 0xFF);
            bytes[2] = (byte)((value >> 16)& 0xFF);
            bytes[3] = (byte)((value >> 24)& 0xFF);
        }
        public static void WriteTo(this long value, Span<byte> bytes)
        {
            // 确保Span<byte>的长度至少为8，因为long类型占用8个字节
            if (bytes.Length != 8) throw new ArgumentException(nameof(bytes));

            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 24) & 0xFF);
            bytes[4] = (byte)((value >> 32) & 0xFF);
            bytes[5] = (byte)((value >> 40) & 0xFF);
            bytes[6] = (byte)((value >> 48) & 0xFF);
            bytes[7] = (byte)((value >> 56) & 0xFF);
        }

        public static long ToLong(this ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 8) throw new ArgumentException(nameof(bytes));

            // 使用 long 类型来避免在位移操作中的溢出
            long num = ((bytes[0])
                        | ((long)bytes[1] << 8)
                        | ((long)bytes[2] << 16)
                        | ((long)bytes[3] << 24));

            long num2 = ((bytes[4])
                         | ((long)bytes[5] << 8)
                         | ((long)bytes[6] << 16)
                         | ((long)bytes[7] << 24));

            // 将 num2 左移 32 位并与 num 进行按位或操作来组合成一个完整的 long 值
            return (num2 << 32) | num;
        }
        
    }
}
