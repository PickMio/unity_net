using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;


namespace net_work
{
    class NetWork
    {
        private bool stop_;

        public bool stop
        {
            get { return stop_; }
            set { stop_ = value; }
        }
        private ManualResetEventSlim recv_event_;
        //所有连接 
        private List<Connection> connections_ ;
        //消息解析
        private MsgParser parser_;
        //包头对象
        private IHeader header_;
        //消息队列
        private Queue<IMsg> recv_message_queue_;
        private Queue<IMsg> send_message_queue_;
        //消息队列的锁
        private readonly object lock_ = new object();
        //主逻辑接口 
        MainLogic logic_;

        public NetWork(MainLogic l)
        {
            stop_ = false;
            connections_ = new List<Connection>();
            recv_event_ = new ManualResetEventSlim();
            parser_ = new MsgParser();
            header_ = new protocols.DDZGameMessage.Header();
            recv_message_queue_ = new Queue<IMsg>();
            send_message_queue_ = new Queue<IMsg>();
            logic_ = l;
        }

        public Connection connect(string ip, int port, int timeout = 10)
        {
            Connection conn = new Connection();
            //TODO 是否需要新建一个connetor 类, 用来返回 connection 对象
            bool ret = conn.connect_to(ip, port, timeout);
            if ( !ret)
            {
                stop = true;
                //连接失败, 返回kong 
                return null;
            }

            connections_.Add(conn);
            return conn;
        }
        public bool send_msg(Connection conn, IMsg msg)
        {
            lock(lock_)
            {
                msg.sock_id = conn.id;
                send_message_queue_.Enqueue(msg);
            }
            return true;
            
        }

        /// <summary>
        /// TODO 读写分别开两个线程感觉没有必要, 徒然增加了复杂性.
        /// 当读线程断开连接时, 此时写线程正在写数据, 该怎么办 ?
        /// TODO 可以把协议解析消息写在底层网络层??
        /// </summary>
        public void start()
        {
            //注册系统消息
            register_sys_message();
            //注册用户消息
            register_user_message();
            ParameterizedThreadStart pnet = net_loop;
            Thread pnetloop = new Thread(pnet);
            pnetloop.Start(this);

            game_loop();

            pnetloop.Join();

        }
        private void on_send_timer(Int64 tm)
        {
            ArrayList datas = new ArrayList(); 
            lock(lock_)
            {
                while( send_message_queue_.Count > 0 )
                {
                    datas.Add(send_message_queue_.Dequeue());
                    
                }
            }
            for (int k = 0; k < datas.Count; ++k )
            {
                IMsg data = (IMsg)datas[k];

                for (int c = 0; c < connections_.Count; ++c)
                {
                    var conn = connections_[c];
                    if (data.sock_id == connections_[c].id)
                    {
                        header_.body_size = data.size;
                        header_.message_type = data.type;
                        conn.send_msg(header_, data);
                        NET_ERROR_CODE err = conn.send_to_peer();

                        if (err != NET_ERROR_CODE.OK)
                        {
                            Logger.log("send msg fail!");
                            connections_.Remove(conn);
                            if (0 == connections_.Count)
                            {
                                stop = true;
                                Logger.log("net loop is stoped because all connection is closed.");
                                //所有连接都断开了
                                return;
                            }
                        }
                        break;
                    }
                }
            }

        }
        private void on_timer(Int64 tm)
        {
            on_send_timer(tm);
        }

        //主线程
        private void game_loop()
        {
            /*
            IHeader h = new DdzNetMsg.Header();
            DdzNetMsg.Authen_Req req = new DdzNetMsg.Authen_Req();
            req.login_type = (char)2;
            req.passwd = "670b14728ad9902aecba32e22fa4f6bd";
            req.roomid = 1;
            req.userid = 30121;
            send_msg(connections_[0], req);
            */
            ArrayList datas = new ArrayList();
            while (!stop_)
            {
                //等待100毫秒
                recv_event_.Wait(100);
                on_logic_timer();
                lock(lock_)
                {
                    while (recv_message_queue_.Count > 0)
                    {
                        datas.Add(recv_message_queue_.Dequeue());
                    }
                }
                for (int c = 0; c < datas.Count; ++c)
                {
                    IMsg msg = (IMsg)datas[c];
                    msg.process();
                }

                recv_event_.Reset();
                
            }

        }

       

        private static void net_loop(object obj)
        {
            NetWork worker = (NetWork)obj;
            
            ArrayList rlist = new ArrayList();
            while (!worker.stop_)
            {
                rlist.Clear();
                
                for (int c = 0; c < worker.connections_.Count; ++c)
                {
                    rlist.Add(worker.connections_[c]);
                    
                }

                //等待网络消息, 等10毫秒超时, 自动发送数据
                Socket.Select(rlist, null, null, 1000 * 1000);
                if (0 == rlist.Count )
                {
                    //这里面做发送处理
                    worker.on_timer(3);
                    continue;
                }
                for (int c = 0; c < rlist.Count; ++c)
                {
                    Connection conn = (Connection)rlist[c];
                    NET_ERROR_CODE err = conn.recv_from_peer();

                    if ( err == NET_ERROR_CODE.NET_ERROR_SOCKET_CLOSED )
                    {
                        lock (worker.lock_)
                        {
                            Logger.log("server is closed!");
                            IMsg close_msg = new SysSocketCloseMessage();
                            close_msg.sock_id = conn.id;
                            worker.recv_message_queue_.Enqueue(close_msg);
                            worker.recv_event_.Set();
                            worker.connections_.Remove(conn);
                            if (0 == worker.connections_.Count)
                            {
                                worker.stop_ = true;
                                return;
                            }
                        }
                    }

                    ArrayList datas = new ArrayList(); 
                    IMsg data = null;
                    do
                    {
                        data = conn.get_msg(worker.header_, worker.parser_);
                        if (null != data)
                        {
                            data.sock_id = conn.id;
                            datas.Add(data);
                        }
                    } while (null != data);
                    
                    lock (worker.lock_)
                    {
                        for (int j = 0; j < datas.Count; ++j)
                        {
                            worker.recv_message_queue_.Enqueue((IMsg)datas[j]);

                        }
                        worker.recv_event_.Set();
                    }
                }
            }
            Logger.log("net work stopped");

        }
        private void register_user_message()
        {
            logic_.reg_msg();
            
        }
        private void register_sys_message()
        {
            IMsg unkonwn = new SysUnkonwnMessage();
            IMsg sockclose = new SysSocketCloseMessage();

            parser_.add(unkonwn);
            parser_.add(sockclose);
        }
        public void on_logic_timer()
        {
            logic_.timer();
        }

        public MsgParser parser 
        {
            get
            {
                return parser_;
            }
        }

    }
}
