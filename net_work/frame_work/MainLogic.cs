using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
    abstract class MainLogic
    {
        public MainLogic()
        {
            worker_ = new NetWork(this);
        }
        
        public NetWork worker_;
        //必须先注册消息, 再 start
        public void start()
        {
            worker_.start();
        }
        //遇到致命错误时停止所有线程
        public void stop_the_world()
        {
            worker_.stop = true;
        }
        abstract public void reg_msg();
        public bool send_msg(Connection conn, IMsg msg)
        {
            return worker_.send_msg(conn, msg);
        }
        abstract public void timer();

        public Connection connect(string ip, int port, int timeout = 10)
        {
            return worker_.connect(ip, port, timeout);
        }

        protected void register_message(IMsg msg)
        {
            worker_.parser.add(msg);

        }
    }
}
