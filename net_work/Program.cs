using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//C# instructions https://docs.microsoft.com/zh-cn/dotnet/articles/csharp/
//threads https://msdn.microsoft.com/zh-cn/library/system.threading.manualresetevent(v=vs.110).aspx
//socket https://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket(v=vs.110).aspx
//collections https://msdn.microsoft.com/zh-cn/library/mt654013.aspx#BKMK_Generic
namespace net_work
{
    class Logger
    {
        public static void log(string msg)
        {
            System.Console.WriteLine(msg);
        }
    }

    class Program
    {
        static void test_buffer()
        {
            SocketBuffer buffer = new SocketBuffer(1024);
            int times = 30;
            Action producer = () =>
            {
                string msg = "good good study";
                Byte[] f = Encoding.UTF8.GetBytes(msg);
                System.Console.WriteLine(f.Length);
                for (int c = 0; c < times; ++c)
                //for (; ; )
                {
                    if (buffer.put_msg(f, f.Length))
                    {
                        Logger.log("put msg ok");

                    }

                }
            };

            Action consumer = () =>
            {
                
                for (int c = 0; c < times; ++c)
                {
                    Byte[] msg = buffer.get_msg(15);
                    if (null != msg)
                    {
                        Logger.log(Encoding.UTF8.GetString(msg));
                    }

                }


            };
            Task tput = new Task(producer);
            tput.Start();
            Task tget = new Task(consumer);
            tget.Start();

            tput.Wait();
            tget.Wait();
        }
        static void test(string[] args)
        {
            /*
            if (args.Length < 3)
            {
                Logger.log("usage:\n name ip port userid\n example: net_work 10.0.160.115 16441 30012");
                System.Console.Read();
                return;
            }
            int id;
            if (!int.TryParse(args[2], out id))
            {
                Logger.log("please type the right id!");
                return;
            }

            string ip = args[0];
            int port = 0;
            if (!int.TryParse(args[1], out port))
            {
                Logger.log("please type the port id!");
                return;
            }

            */
            /*
            string ip = "92.11.11.12";
            //string ip = "10.0.160.115";
            int port = 16441;
            int id = 30120;

            Connection conn = new Connection();
            bool ret = conn.connect_to(ip, port, 3);
            if (false == ret)
            {
                Logger.log("connect to server fail!");
                return;
            }
            DdzNetMsg.Authen_Req req = new DdzNetMsg.Authen_Req();
            req.login_type = (char)2;
            req.passwd = "670b14728ad9902aecba32e22fa4f6bd";
            req.roomid = 1;
            req.userid = id;
            conn.send_msg(req);

            Thread.Sleep(3000);

            DdzNetMsg.SitdownReq sitreq = new DdzNetMsg.SitdownReq();
            conn.send_msg(sitreq);

            DdzNetMsg.ReadyReq rreq = new DdzNetMsg.ReadyReq();
            conn.send_msg(rreq);
            */
        }

        static void test_frame()
        {
            /*
            NetWork nt = new NetWork(this);
            //if ( null == nt.connect("q.pickmio.com", 7737) )
            if ( null == nt.connect("10.0.160.115", 16441) )
            {
                Logger.log("connect to server fail");
                return;
            }
            nt.start();
            */
            DDZGameLogic logic = new DDZGameLogic();
            logic.init();
            logic.start();
           

            


        }
        static void Main(string[] args)
        {
            //test( args);
            test_frame();
            //test_buffer();
            System.Console.Read();
        }
    }
}
