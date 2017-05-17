using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net_work.protocols.DDZGameMessage;

using System.IO;

using MsgPack;
using MsgPack.Serialization;

namespace net_work.logic
{
    public class OnLoginReq : IMsg
    {
        public OnLoginReq()
        {
            type_ = 0x0001;
            msg_ = new LoginReq();
        }
        public override IMsg create_obj()
        {
            return new OnLoginReq();
        }
        public override byte[] encode()
        {
            var stream = new MemoryStream();
            var serial = MessagePackSerializer.Get<LoginReq>();
            serial.Pack(stream, msg_);
            Byte[] data = new Byte[stream.Length];
            Array.Copy(stream.GetBuffer(), data, stream.Length);


            size_ = (int)stream.Length;
            
            return data;

        }
        public override void decode()
        {
        }
        public override void process()
        {
        }

        public LoginReq msg_;
    }
}
