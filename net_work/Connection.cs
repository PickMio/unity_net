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
    class Connection
    {
        public Connection()
        {
            status_ = NET_STATUS.NET_STATUS_NONE;
            reconn_times_ = 0;
            timeout_ = 0;
        }
        //给逻辑线程的发送接口
        public bool send_msg(IMsg msg)
        {
            Byte[] message = msg.encode();
            //send_buffer_.put_msg(message);
            int sz = peer_.Send(message);
            if (sz != message.Length)
            {
                Logger.log("error, send length is not match!");
                return false;
            }

            return true;
        }
        /// <summary>
        /// 发送线程
        /// </summary>
        public void send_loop()
        {
        }

        /// <summary>
        /// 接收线程
        /// </summary>
        public void recv_loop()
        {

        }
        /// <summary>
        /// 给逻辑线程的接收接口
        /// </summary>
        /// <param name="msg">发送消息</param>
        /// <param name="sz">长度</param>
        /// <returns></returns>
        public bool on_net_message()
        {
            
            return false;
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
        /// <param name="timeout">超时限定, 单位秒, 默认10秒</param>
        /// <returns></returns>
        public bool connect_to(string ip, int port, int timeout = 10)
        {
            peer_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peer_.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), this);
            status_ = NET_STATUS.NET_STATUS_CONNECTING;

            ip_ = ip;
            port_ = port;
            timeout_ = timeout;
            
            Logger.log("begin connecting to server");
            if (timeout_obj_.WaitOne(timeout * 1000, false))
            {
                if (status_ != NET_STATUS.NET_STATUS_ESTABLISHED)
                {
                    return false;
                }
            }           

            // log connecting
            return true;
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
            Connection conn = (Connection)ar.AsyncState;
            try 
            {                 
                conn.peer.EndConnect(ar);
                conn.status_ = NET_STATUS.NET_STATUS_ESTABLISHED;
                conn.on_connected();                
                
            }
            catch(System.Net.Sockets.SocketException)
            {
                Logger.log("connect fail, try to reconnect!");
                conn.status_ = NET_STATUS.NET_STATUS_RECONNECT;                
                
            }
            finally
            {
                conn.timeout_obj_.Set();
            }            
            
            
        }

        public Socket peer
        {
            get
            {
                return peer_;
            }
        }

        public ManualResetEvent timeout_obj_ = new ManualResetEvent(false);
        //连接socket
        private Socket peer_;
        
        //发送缓冲
        private SocketBuffer send_buffer_;
        //接收缓冲
        private SocketBuffer recv_buffer_;
        private string ip_;
        private int port_;
        private int timeout_;
        private NET_STATUS status_;
        private int reconn_times_;
        
    }
}
