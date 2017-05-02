using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        static void test()
        {
            Connection conn = new Connection();
            conn.connect_to("139.199.16.208", 7712, 3);
        }
        static void Main(string[] args)
        {
            test();
        }
    }
}
