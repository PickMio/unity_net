using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work.protocols
{
    namespace DDZGameMessage
    {
        class Header : IHeader
        {
            public Header()
            {
                head_size_ = 8;


                version_ = 0x99;
                encode_ = 0x03; //message pack
                reserve_ = 0x00;
                state_ = 0x00;
                length_ = 0x00; //需要填充, 包数据大小, 不包含包头
                type_ = 0x00;   //需要填充, 
            }
            override public Byte[] encode()
            {
                // 打包头结点
                length_ = (short)body_size;
                type_ = (short)message_type;

                Byte[] data = new Byte[8];
                NetWorkUtils.put_byte(data, 0, version_);
                NetWorkUtils.put_byte(data, 1, encode_);
                NetWorkUtils.put_byte(data, 2, reserve_);
                NetWorkUtils.put_byte(data, 3, state_);
                NetWorkUtils.put_int16(data, 4, length_, false);
                NetWorkUtils.put_int16(data, 6, type_, false);
                return data;
            }
            override public void decode()
            {
                NetWorkUtils.get_byte(message, 0, out version_);
                NetWorkUtils.get_byte(message, 1, out encode_);
                NetWorkUtils.get_byte(message, 2, out reserve_);
                NetWorkUtils.get_byte(message, 3, out state_);
                NetWorkUtils.get_int16(message, 4, out length_, false);
                NetWorkUtils.get_int16(message, 6, out type_, false);

                body_size = length_;
                message_type = type_;

                System.Console.WriteLine("decode version:{0:X} encode{1:X} reserve{2:X} state:{3:X} length:{4:X} type:{5:X}",
                                            version_, encode_, reserve_, state_, length_, type_);

            }


            private byte version_;
            private byte encode_;
            private byte reserve_;
            private byte state_;
            private short length_;
            private short type_;



        }
        public class LoginReq 
        {
            public string name { get; set; }
            public string passwd { get; set; }
            public Int32 platform { get; set; }
        }
        public class LoginRsp
        {
            public Int32 err { get; set; }
            public Int32 id { get; set; }
            public Int32 gender { get; set; }
            public string sign { get; set; }
        }




    }

}
