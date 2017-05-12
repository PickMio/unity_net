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
            IMsg authen = new DdzNetMsg.Authen_Req();
            IMsg sit = new DdzNetMsg.SitdownReq();
            IMsg ready = new DdzNetMsg.ReadyReq();
            register_message(authen);
            register_message(sit);
            register_message(ready);
        }

        //每10微秒刷新的定时器
        override public void timer()
        {
            Logger.log("on timer");

        }

        //初始化一些东西, 如连接服务器, 当连接成功后, 主线程要调用 start函数开始所有逻辑
        public void init()
        {            
            ddz_svr_ = connect("10.0.160.115", 16441);
            if (ddz_svr_ == null)
            {
                Logger.log("connect to server fail");
                return;
            }

        }
    }
}

