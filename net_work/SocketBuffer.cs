using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace net_work
{
    /// <summary>
    /// 是线程安全的类
    /// </summary>
    class SocketBuffer
    {
        private Byte[] buffer_;
        private int max_buff_sz_;
        private int start_pos_;
        private int end_pos_;
        private readonly object lock_ = new object();

        public SocketBuffer(int max_sz)
        {
            max_buff_sz_ = max_sz;
            buffer_ = new Byte[max_buff_sz_];
        }

        /// <summary>
        /// 将数据放入缓冲区
        /// </summary>
        /// <param name="msg">消息的byte数组</param>
        /// <param name="sz">消息的实际长度</param>
        /// <returns>成功返回true, 失败返回false</returns>
        public bool put_msg(Byte[] msg, int sz)
        {
            //lock (lock_)
            {
                if (sz > msg.Length)
                {
                    //log error 发送参数sz不对
                    return false;
                }
                
                int free_sz = max_buff_sz_ - (end_pos_ - start_pos_);

                //buffer不够放
                if (sz > free_sz)
                {
                    //log errror, buffer is full
                    return false;
                }

                //数据尾端到最大buff不够装下msg , 需要移动数据到包头, 再添加msg
                if (sz > max_buff_sz_ - end_pos_)
                {
                    //log 发送缓冲区超出限制
                    move_buff_to_head();
                }

                Array.Copy(msg, 0, buffer_, end_pos_, sz);                
                end_pos_ += sz;
                //System.Console.WriteLine("put msg success, msglen[{0}] startpos[{1}] endpos[{2}]\n\n\n", sz, start_pos_, end_pos_);
                return true;

            }
        }

        /// <summary>
        /// 从缓冲区获取消息 
        /// </summary>
        /// <param name="sz">所需要的消息长度, 多少字节</param>
        /// <returns>成功返回获取的字符数, 失败返回null</returns>
       
        public Byte[] get_msg(int sz, bool send = false)
        {
            //lock (lock_)
            {
                int total = end_pos_ - start_pos_;

                if (0 == total)
                {
                    //一字节数据都没有
                    return null;
                }
                if ( total < sz)
                {                    
                    if (false == send)
                    {
                        //log error 还没有需要的数据
                        return null;
                    }

                    sz = end_pos_ - start_pos_;                    
                }
                Byte[] msg = new Byte[sz];
                Array.Copy(buffer_, start_pos_, msg, 0, sz);
                start_pos_ += sz;
                System.Console.WriteLine("after get msg[{0}] startpos[{1}] endpos[{2}]\n\n\n", sz, start_pos_, end_pos_);
                if (start_pos_ == end_pos_)
                {
                    start_pos_ = end_pos_ = 0;
                }                
                return msg;
            }
        }

        /// <summary>
        /// 将数据移动到buffer 头, 只能在put_msg 或 get_msg 函数的lock中调用
        /// 如果缓冲区足够, 但是缓冲区尾部不够放入数据, 将所有数据移动到 0, 再在后面写入数据
        /// </summary>
        private void move_buff_to_head()
        {
            int data_sz = end_pos_ - start_pos_;

            System.Console.WriteLine("need to move, datasz[{0}] startpos[{1}] endpos[{2}]", data_sz, start_pos_, end_pos_);
            Array.Copy(buffer_, start_pos_, buffer_, 0, data_sz );
            start_pos_ = 0;
            end_pos_ = data_sz;

        }

        
    }
}
