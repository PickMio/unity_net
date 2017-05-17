using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace net_work
{
    enum NET_STATUS
    {
        NET_STATUS_NONE,
        NET_STATUS_CONNECTING,
        NET_STATUS_ESTABLISHED,
        NET_STATUS_CLOSED,
        NET_STATUS_RECONNECT,
    }
    enum NET_ERROR_CODE
    {
        OK,
        NET_ERROR_NO_DATA,
        NET_ERROR_SOCKET_CLOSED,
        NET_ERROR_SEND_FAIL,
        NET_ERROR_RECEIVE_FAIL,
        NET_ERROR_NOT_ESTABLISHED,

    }


    /// <summary>
    /// 一个Connection 表示一条连接, 继承自 Socket
    /// </summary>
    class Connection : Socket
    {
        public Connection()
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            status_ = NET_STATUS.NET_STATUS_NONE;
            reconn_times_ = 0;
            timeout_ = 0;
            timeout_obj_ = new ManualResetEvent(false);
            recv_buffer_ = new SocketBuffer(1024 * 16);
            send_buffer_ = new SocketBuffer(1024 * 16);
            id_ = ++connection_id_generator_;

        }
        //给逻辑线程的发送接口
        //TODO 把装包放在接收发送线程
        public bool send_msg(IHeader h, IMsg msg)
        {
            // TODO 这里可以考虑把消息放入msg,在msg malloc 时把头结点的长度包含进来
          
            // message 要先encode, 算出包体大小, 再encode 包头
            Byte[] message = msg.encode();
            h.message_type = msg.type;
            h.body_size = msg.size;
            Byte[] header = h.encode();
            ;
           
            Byte[] data = new Byte[header.Length + message.Length];
            header.CopyTo(data, 0);
            message.CopyTo(data, header.Length);
            return send_buffer_.put_msg(data, data.Length); 
            
        }
        //从缓存中取出消息
        public IMsg get_msg( IHeader header, MsgParser parser)
        {
            if (header.status_ == IHeader.MSG_STATUS.MSG_STATUS_HEAD)
            {
                Byte[] h = recv_buffer_.get_msg(header.head_size);
                if ( null == h )
                {
                    return null;
                }
                header.message = h;
                header.decode();
                header.status_ = IHeader.MSG_STATUS.MSG_STATUS_BODY;
            }
            Byte[] msg = recv_buffer_.get_msg(header.body_size);
            if (null == msg)
            {
                return null;
            }
            header.status_ = IHeader.MSG_STATUS.MSG_STATUS_HEAD;

            IMsg data = parser.parse_msg(header, msg);
            if (null == data)
            {
                //未知消息 
                data = new SysUnkonwnMessage();
                System.Console.WriteLine("receive unknown message, id {0:X}", header.message_type);
                return data;
            }

            data.decode();
            return data;
        }
        /// <summary>
        /// 发送线程
        /// </summary>
        public NET_ERROR_CODE send_to_peer()
        {
            int sendsz = 0;
            if (status_ != NET_STATUS.NET_STATUS_ESTABLISHED)
            {
                //log error, socket status not right
                return NET_ERROR_CODE.NET_ERROR_NOT_ESTABLISHED;
            }


            //一次最多发送4096字节,超过的自动拆包
            Byte[] msg = send_buffer_.get_msg(MAX_BUFFER_LENGTH, true);
            if( null == msg )
            {
                return NET_ERROR_CODE.OK;
            }
            while (sendsz < msg.Length)
            {
                try
                {
                    int sz = this.Send(msg);
                    sendsz += sz;
                }
                catch (SocketException e)
                {
                    System.Console.WriteLine("send fail , socket error code[{0}]", e.ErrorCode);
                    return NET_ERROR_CODE.NET_ERROR_SEND_FAIL;
                }
                catch(ObjectDisposedException )
                {
                    System.Console.WriteLine("send fail , socket has been closed, error code[{0}]");
                    return NET_ERROR_CODE.NET_ERROR_SOCKET_CLOSED;
                }                

            }


            return NET_ERROR_CODE.OK;
        }

        /// <summary>
        /// 接收线程
        /// </summary>
        public NET_ERROR_CODE recv_from_peer()
        {
            try
            {
                int sz = 0;
                sz = this.Receive(buffer_);

                if (0 == sz)
                {
                    //连接断开
                    return NET_ERROR_CODE.NET_ERROR_SOCKET_CLOSED;

                }

                //这里buffer 满了会将数据丢弃...应该阻塞等待
                recv_buffer_.put_msg(buffer_, sz);

            }
            catch (SocketException )
            {
                //log 有错误发生
                return NET_ERROR_CODE.NET_ERROR_RECEIVE_FAIL;
            }


            return NET_ERROR_CODE.OK;

        }


        public void on_reconn()
        {
            if (reconn_times_ >= 7)
            {
                return;
            }
            ++reconn_times_;
            this.connect_to(ip_, port_, timeout_);            
            return;
        }
        
        /// <summary>
        /// 连接服务器
        /// 默认断开后尝试重连, 尝试重连7次, 第一次间隔1秒, 第二次2秒, 第三次4秒, 第四次8秒, 第五次16秒, 第六次32秒, 第七次64秒
        /// </summary>
        /// <param name="ip">服务器地址</param>
        /// <param name="port">服务器端口号</param>
        /// <param name="timeout">超时限定, 单位秒</param>
        /// <returns></returns>
        public bool connect_to(string ip, int port, int timeout)
        {
            //peer_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), this);
            status_ = NET_STATUS.NET_STATUS_CONNECTING;

            ip_ = ip;
            port_ = port;
            timeout_ = timeout;
            
            Logger.log("begin connecting to server");
            //这个是在主线程阻塞 timeout, 如果 BeginConnect 连接成功会设置连接状态... 多个线程使用status_ 可能会出现问题.但概率不大
            if (timeout_obj_.WaitOne(timeout * 1000, false))
            {
                //收到set 信号会返回 true
                if (status_ == NET_STATUS.NET_STATUS_ESTABLISHED)
                {
                    Logger.log("connection established!");
                    return true;
                }

                //关闭连接
                //peer_.Close();
                // 状态不对
                return false;
            }

            //ConnectCallback 没有执行
            this.Close();
            //not connected
            return false;
        }
        public void on_connected()
        {
            //Logger.log("connect to server success\n");
            //string data = "good job";
            //Byte[] msg = Encoding.UTF8.GetBytes(data);
            
            //peer_.Send(msg);
            //log connected
        }
        public static void ConnectCallback(IAsyncResult ar)
        {
            Logger.log("connection callback");
            Connection conn = (Connection)ar.AsyncState;
            try 
            {                 
                conn.EndConnect(ar);
                conn.status_ = NET_STATUS.NET_STATUS_ESTABLISHED;
                conn.on_connected();                
                
            }
            catch(System.Net.Sockets.SocketException)
            {
                Logger.log("connect fail, try to reconnect!");
                conn.status_ = NET_STATUS.NET_STATUS_RECONNECT;                
                
            }
            catch (System.ObjectDisposedException)
            {
                Logger.log("connection timeout and canceled");
            }
            finally
            {
                conn.timeout_obj_.Set();
            }            
            
            
        }


        public int id
        {
            get
            {
                return id_;
            }
        }

        public const int MAX_BUFFER_LENGTH = 4096;
        public ManualResetEvent timeout_obj_ ;
        
        
        //发送缓冲
        private SocketBuffer send_buffer_;
        //接收缓冲
        private SocketBuffer recv_buffer_;
        private string ip_;
        private int port_;
        private int timeout_;
        private NET_STATUS status_;
        private int reconn_times_;
        //用来唯一标识这个连接
        private int id_;

        static int connection_id_generator_ = 0;
        //只能有一个接收线程, 这个buffer_ 不是线程安全的
        //接收的buffer
        private static Byte[] buffer_ = new Byte[MAX_BUFFER_LENGTH];
        
        
    }
}
