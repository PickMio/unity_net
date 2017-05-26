using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
    class NetWorkUtils
    {
        public static bool get_byte(Byte[] src, int start, out Byte data)
        {
            if (src.Length < start + 1)
            {
                data = (Byte)0;
                return false;
            }

            data = src[start];
            return true;
        }
        public static bool get_int16(Byte[] src, int start, out Int16 data, bool reverse = false)
        {
            data = 0;
            if (src.Length < start + 2)
            {
                return false;
            }
            if (reverse)
            {
                data |= ((Int16)src[start + 1]);
                data |= (Int16)(((Int16)src[start + 0]) << 8);
            }
            else
            {
                //中间生成的值可能为 int32
                data |= ((Int16)src[start + 0]);
                data |= (Int16)(((Int16)src[start + 1]) << 8);
       
            }

            return true; ;
        }
        public static bool get_int32(Byte[] src, int start, out Int32 data, bool reverse = false)
        {
            data = 0;
            if (src.Length < start + 4)
            {
                return false;
            }
            if (reverse)
            {
                data |= (Int32)((Int32)src[start + 3]) ;
                data |= (Int32)(((Int32)src[start + 2]) << 8);
                data |= (Int32)(((Int32)src[start + 1]) << 16);
                data |= (Int32)(((Int32)src[start]) << 24);

            }
            else
            {
                data |= (Int32)((Int32)src[start]);
                data |= (Int32)(((Int32)src[start + 1]) << 8);
                data |= (Int32)(((Int32)src[start + 2]) << 16);
                data |= (Int32)(((Int32)src[start + 3]) << 24);

            }

            return true; 

        }
        public static bool get_int64(Byte[] src, int start, out Int64 data, bool reverse = false)
        {

            data = 0;
            if (src.Length < start + 8)
            {
                return false;
            }
            if (reverse)
            {
                data |= (Int64)((Int64)src[start + 7]);
                data |= (Int64)(((Int64)src[start + 6]) << 8);
                data |= (Int64)(((Int64)src[start + 5]) << 16);
                data |= (Int64)(((Int64)src[start + 4]) << 24);


                data |= (Int64)(((Int64)src[start + 3]) << 32);
                data |= (Int64)(((Int64)src[start + 2]) << 40);
                data |= (Int64)(((Int64)src[start + 1]) << 48);
                data |= (Int64)(((Int64)src[start]) << 56);  
            }
            else
            {
                //必须先转换成 int64 再位移, 不然 src[start + 4]) << 32 会被当成 int32 来位移, 会溢出 
                data |= (Int64)((Int64)src[start]);
                data |= (Int64)(((Int64)src[start + 1]) << 8);
                data |= (Int64)(((Int64)src[start + 2]) << 16);
                data |= (Int64)(((Int64)src[start + 3]) << 24);


                data |= (Int64)(((Int64)src[start + 4]) << 32);
                data |= (Int64)(((Int64)src[start + 5]) << 40);
                data |= (Int64)(((Int64)src[start + 6]) << 48);
                data |= (Int64)(((Int64)src[start + 7]) << 56); 

              
               
            }

            return true; 
        }
        //字符串是utf-8 格式的
        public static bool get_string(Byte[] src, out string dest, int start, int size)
        {
            dest = "";
            if (src.Length < start + size * 2)
            {
                return false;
            }
            dest = Encoding.UTF8.GetString(src, start, size);
            return true;
        }

        public static bool put_byte(Byte[] src, int dest, Byte data)
        {
            if (src.Length <= dest)
            {
                return false;
            }
            src[dest] = data;
            return true;

        }
        /// <summary>
        /// 从data读入一个 Int16 型数据到 src 的 dest 后
        /// </summary>
        /// <param name="src">要拷贝到的byte数组</param>
        /// <param name="dest">拷贝到数组的位置</param>
        /// <param name="data">数据</param>
        /// <param name="reverse">是否反转</param>
        /// <returns></returns>
        public static bool put_int16(Byte[]src, int dest, Int16 data, bool reverse = false)
        {
            if (src.Length < dest + 2)
            {
                return false;
            }
            Byte[] bdata = BitConverter.GetBytes(data);
            if ( reverse )
            {
                Array.Reverse(bdata);
            }
            Array.Copy(bdata, 0, src, dest, 2);
            return true;

        }
        public static bool put_int32(Byte[]src, int dest, Int32 data, bool reverse = false)
        {
            if (src.Length < dest + 4)
            {
                return false;
            }
            Byte[] bdata = BitConverter.GetBytes(data);
            if ( reverse )
            {
                Array.Reverse(bdata);
            }
            Array.Copy(bdata, 0, src, dest, 4);
            return true;

        }
        public static bool put_int64(Byte[]src, int dest, Int64 data, bool reverse = false)
        {
            if (src.Length < dest + 8)
            {
                return false;
            }
            Byte[] bdata = BitConverter.GetBytes(data);
            if ( reverse )
            {
                Array.Reverse(bdata);
            }
            Array.Copy(bdata, 0, src, dest, 8);
            return true;

        }
        public static bool put_string(Byte[]src, int dest, string data, bool reverse = false)
        {
            if (src.Length < dest + data.Length )
            {
                return false;
            }
            Byte[] bstr = Encoding.UTF8.GetBytes(data);
            Array.Copy(bstr, 0, src, dest, bstr.Length);

            return true;

        }
    }
}
