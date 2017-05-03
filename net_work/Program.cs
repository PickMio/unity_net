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
        static void test(int userid = 30120)
        {
            Connection conn = new Connection();
            conn.connect_to("10.0.160.115", 19103, 3);
            DdzNetMsg.Authen_Req req = new DdzNetMsg.Authen_Req();
            req.login_type = (char)2;
            req.passwd = "670b14728ad9902aecba32e22fa4f6bd";
            req.roomid = 1;
            req.userid = userid;
            conn.send_msg(req);

            Thread.Sleep(3000);
            
            DdzNetMsg.SitdownReq sitreq = new DdzNetMsg.SitdownReq();
            conn.send_msg(sitreq);
            
            DdzNetMsg.ReadyReq rreq = new DdzNetMsg.ReadyReq();
            conn.send_msg(rreq);
        }
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                test();
                System.Console.Read();
                return;
            }
            int id;
            if (!int.TryParse(args[0], out id))
            {
                id = 30120;

            }
            test(id);
            System.Console.Read();
        }
    }
}
