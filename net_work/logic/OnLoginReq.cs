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
            login_msg_ = new LoginReq();
        }
        public override IMsg create_obj()
        {
            return new OnLoginReq();
        }
        public override byte[] encode()
        {
            var stream = new MemoryStream();
            var serial = MessagePackSerializer.Get<LoginReq>();
            serial.Pack(stream, login_msg_);
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

        public LoginReq login_msg_;
    }


    public class OnLoginRsp: IMsg
    {
        public OnLoginRsp()
        {
            type_ = 0x0002;
            login_msg_ = new LoginRsp();
        }
        public override IMsg create_obj()
        {
            return new OnLoginRsp();
        }
        public override byte[] encode()
        {
            var stream = new MemoryStream();
            var serial = MessagePackSerializer.Get<OnLoginRsp>();
            serial.Pack(stream, login_msg_);
            Byte[] data = new Byte[stream.Length];
            Array.Copy(stream.GetBuffer(), data, stream.Length);


            size_ = (int)stream.Length;

            return data;

        }
        public override void decode()
        {
            var serial = MessagePackSerializer.Get<LoginRsp>();
            var stream = new MemoryStream();
            stream.Write(message, 0, (int)size);
            stream.Position = 0;
            login_msg_ = serial.Unpack(stream);
        }
        public override void process()
        {
            System.Console.WriteLine("authen success, err:{0}, id:{1}, gender:{2}, sign:{3}", login_msg_.err, login_msg_.id, login_msg_.gender, login_msg_.sign);
        }

        public LoginRsp login_msg_;
    }
}
  