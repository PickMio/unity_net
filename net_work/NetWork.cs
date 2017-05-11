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
        public NetWork()
        {
            stop_ = false;
            connections_ = new List<Connection>();
            recv_event_ = new ManualResetEventSlim();
            parser_ = new MsgParser();
            header_ = new DdzNetMsg.Header();
        }

        public Connection connect(string ip, int port, int timeout = 10)
        {
            Connection conn = new Connection();
            //TODO 是否需要新建一个connetor 类, 用来返回 connection 对象
            bool ret = conn.connect_to(ip, port, timeout);
            if ( !ret)
            {
                //连接失败, 返回kong 
                return null;
            }

            connections_.Add(conn);
            return conn;
        }
        public bool send_msg(Connection conn, IMsg msg)
        {
            for (int c = 0; c < connections_.Count; ++c)
            {
                if( conn.id == connections_[c].id )
                {
                    header_.body_size = msg.size;
                    header_.message_type = msg.type;
                    conn.send_msg(header_, msg);
                    return true;
                }
            }
            //连接不存在
            return false;

        }

        /// <summary>
        /// TODO 读写分别开两个线程感觉没有必要, 徒然增加了复杂性.
        /// 当读线程断开连接时, 此时写线程正在写数据, 该怎么办 ?
        /// TODO 可以把协议解析消息写在底层网络层??
        /// </summary>
        public void start()
        {
            ParameterizedThreadStart pnet = net_loop;
            Thread pnetloop = new Thread(pnet);
            pnetloop.Start(this);

            //注册系统消息
            register_sys_message();
            //注册用户消息
            register_user_message();
            game_loop();

            pnetloop.Join();

        }
        private void on_send_timer(Int64 tm)
        {
            for (int c = 0; c < connections_.Count; ++c)
            {
                //将数据发送给服务器
                Connection conn = connections_[c];
                if (null == conn)
                {
                    //没有连接了
                    return;
                }
                NET_ERROR_CODE err = conn.send_to_peer();

                if (err != NET_ERROR_CODE.OK)
                {
                    Logger.log("send msg fail!");
                    connections_.Remove(conn);
                    if (0 == connections_.Count)
                    {
                        Logger.log("net loop is stoped because all connection is closed.");
                        //所有连接都断开了
                        return;
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
            IHeader h = new DdzNetMsg.Header();
            DdzNetMsg.Authen_Req req = new DdzNetMsg.Authen_Req();
            req.login_type = (char)2;
            req.passwd = "670b14728ad9902aecba32e22fa4f6bd";
            req.roomid = 1;
            req.userid = 30121;

            connections_[0].send_msg(h, req);
            while (true)
            {
                //等待500毫秒
                recv_event_.Wait();
                for (int c = 0; c < connections_.Count; ++c )
                {

                    Connection conn = connections_[c];
                    IMsg data = null;
                    do
                    {
                        data = conn.get_msg(h, parser_);
                        if (null != data)
                        {
                            data.process();
                        }
                    } while (null != data);
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
                Socket.Select(rlist, null, null, 10 * 1000);
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
                    Logger.log("receive data from peer");
                    if ( err == NET_ERROR_CODE.NET_ERROR_SOCKET_CLOSED )
                    {
                        Logger.log("server is closed!");
                        worker.connections_.Remove(conn);
                        if (0 == worker.connections_.Count)
                        {
                            //所有连接都断开了
                            return;
                        }
                    }

                    worker.recv_event_.Set();
                }
            }

        }
        private void register_user_message()
        {
            IMsg authen = new DdzNetMsg.Authen_Req();
            IMsg sit = new DdzNetMsg.SitdownReq();
            IMsg ready = new DdzNetMsg.ReadyReq();
            IMsg unkonwn = new SysUnkonwnMessage();
            IMsg sockclose = new SysSocketCloseMessage();

            parser_.add(authen);
            parser_.add(sit);
            parser_.add(ready);
        }
        private void register_sys_message()
        {
            IMsg unkonwn = new SysUnkonwnMessage();
            IMsg sockclose = new SysSocketCloseMessage();

            parser_.add(unkonwn);
            parser_.add(sockclose);
        }


        private bool stop_;
        private ManualResetEventSlim recv_event_;
        //所有连接 
        private List<Connection> connections_ ;
        //消息解析
        private MsgParser parser_;
        private IHeader header_;

    }
}
