using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
    class DDZGameLogic : MainLogic
    {
        Connection ddz_svr_;

        // 在这里面注册所有消息
        public override void reg_msg()
        {
            IMsg login = new logic.OnLoginReq();
            register_message(login);

        }

        //每10微秒刷新的定时器
        override public void timer()
        {
            Logger.log("on timer");
            logic.OnLoginReq login = new logic.OnLoginReq();
            login.msg_.name = "詹姆斯";
            login.msg_.passwd = "123456";
            login.msg_.platform = 18;
            
            send_msg(ddz_svr_, login);

        }

        //初始化一些东西, 如连接服务器, 当连接成功后, 主线程要调用 start函数开始所有逻辑
        public void init()
        {            
            ddz_svr_ = connect("192.168.0.128", 30077);
            if (ddz_svr_ == null)
            {
                Logger.log("connect to server fail");
                return;
            }
            

        }
    }
}

