using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace net_work
{
    /// <summary>
    /// 最大发送缓存为65535字节
    /// 是线程安全的类
    /// </summary>
    class SocketBuffer
    {
        SocketBuffer()
        {
            max_buff_sz_ = 65535;
            buffer_ = new ArrayList();
        }
        public bool put_msg(Byte[] msg)
        {
            if (msg.Length > max_buff_sz_ - buffer_.Count)
            {
                //log 发送缓冲区超出限制
                return false;
            }
            buffer_.Add(msg);

            return true;
        }

        //需要线程同步
        public Byte[] get_msg()
        {
            if (buffer_.Count > 0)
            {
                return (Byte[])buffer_[0];

            }
            return null;
        }
        public bool remove_top()
        {
            if (buffer_.Count <= 0)
            {
                //error array is null
                return false;
            }
            buffer_.RemoveAt(0);
            return true;

        }


        private ArrayList buffer_;
        private int max_buff_sz_;
    }
}
