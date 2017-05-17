using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
    class MsgParser
    {

        public MsgParser()
        {
            message_dic_ = new Dictionary<int,IMsg>();
        }
        public void add(IMsg msg)
        {
            if (null == msg)
            {
                //log error
                return;
            }
            if( message_dic_.ContainsKey( msg.type ) )
            {
                //key duplicated
                message_dic_.Remove( msg.type );
                   
            }
            message_dic_.Add( msg.type, msg );
            //log register message id ok
        }
        public IMsg parse_msg(IHeader h, Byte[] msg)
        {
            //TODO, 如果收到未知消息或者断开消息, 应该添加类型并形成消息给上层
            IMsg m = null;
            if( !message_dic_.TryGetValue(h.message_type, out m ))
            {
                //receive unkown message id
                return null;
            }
            IMsg data = m.create_obj();
            data.message = msg;
            return data;
        }

        //所有消息的创建更新函数在这
        Dictionary<int, IMsg> message_dic_;
    }
}
