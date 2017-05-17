using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace net_work
{
    namespace DdzNetMsg
    {
        class Header : IHeader 
        {
            
            public Header()
            {
                identity_ = 0x05;
                encode_ = 0x00;
                length_ = 8;
                version_   = 0x03;
                reserve_ = 0x00;
                type_ = 0x00;

                //每个header 类型都要设置包头大小
                head_size_ = 8;

            }
            public int type
            {
                set
                {
                    type_ = (short)value;
                }
            }
            public int length 
            {
                set
                {
                    length_ = (short)value;
                }
            }
            
            
            private byte identity_;
            private byte encode_;
            private short length_;
            private byte version_;
            private byte reserve_;
            private short type_;
            public override Byte[] encode()
            {

                length_ = (short)(body_size + head_size);
                type_ = (short)message_type;
                //Encoding.ASCII.GetBytes
                Byte[] blength = BitConverter.GetBytes(length_);
                Array.Reverse(blength);
                Byte[] btype = BitConverter.GetBytes(type_);
                Array.Reverse(btype);

                int sz = 8;

                Byte[] arr = new Byte[sz];
                arr[0] = identity_;
                arr[1] = encode_;
                //bidentiy.CopyTo(arr, 0);
                blength.CopyTo(arr, 2);
                arr[4] = version_;
                arr[5] = reserve_; 
                btype.CopyTo(arr, 6);
                
                return arr;
            }
            //decode 后要给 type 和size 赋值
            override public void decode()
            {
                identity_ = message[0];
                encode_ = message[1];

                Byte[] blen = new Byte[2];
                Array.Copy(message, 2, blen, 0, 2);
                Array.Reverse(blen);
                length_ = BitConverter.ToInt16(blen, 0);
                
                version_ = message[4];
                reserve_ = message[5];

                Byte[] btype = new Byte[2];
                Array.Copy(message, 6, btype , 0, 2);
                Array.Reverse(btype);
                type_ = BitConverter.ToInt16(btype, 0);
                body_size = length_ - head_size;
                message_type = type_;
                System.Console.WriteLine("identi {0} encode {1} length{2} version{3} reserve{4} type{5:X}", identity_, encode_, body_size, version_, reserve_, type_);

                

            }
        }
        class Authen_Req:IMsg 
        {
            public Authen_Req()
            {
                size_ = 56;
                type_ = 0xA0;

            }
            public override IMsg create_obj()
            {
                return new Authen_Req();
            }
            public override Byte[] encode()
            {
                
                Byte[] buserid = BitConverter.GetBytes(userid_);
                Byte[] broomid = BitConverter.GetBytes(roomid_);
                Byte[] bpasswd = new Byte[33];

                var msg = new List<byte>();
                for (int c = 0; c < 33; ++c)
                {
                    if (c < passwd_.Length) 
                    {
                        msg.Add(Convert.ToByte(passwd_[c]));
                    }
                    else
                    {
                        msg.Add(Convert.ToByte((char)0));
                    }

                }
                bpasswd = msg.ToArray<byte>();
                Byte[] blogin_type = BitConverter.GetBytes(login_type_);

                int sz = 56;

                Byte[] arr = new Byte[sz];
                buserid.CopyTo(arr, 0);
                broomid.CopyTo(arr, buserid.Length);
                bpasswd.CopyTo(arr, buserid.Length + broomid.Length);
                blogin_type.CopyTo(arr, 52);

                return arr;
            }
            override public void decode()
            {
                

            }
            override public void process()
            {

            }
            private Int32 userid_;

            public Int32 userid
            {
                get { return userid_; }
                set { userid_ = value; }
            }
            private Int32 roomid_;

            public Int32 roomid
            {
                get { return roomid_; }
                set { roomid_ = value; }
            }
            private string passwd_;//32字节

            public string passwd
            {
                get { return passwd_; }
                set { passwd_ = value; }
            }
            private char login_type_;

            public char login_type
            {
                get { return login_type_; }
                set { login_type_ = value; }
            }

        }
        class SitdownReq:IMsg 
        {
            public SitdownReq()
            {
                type_ = 0xA4;
                size_ = 24;

            }
            public override IMsg create_obj()
            {
                return new SitdownReq();
            }
            override public void process()
            {

            }

            override public Byte[] encode()
            {
                Byte[] bbuserid = BitConverter.GetBytes(bind_userid);
                Byte[] bbtnum = BitConverter.GetBytes(table_num);
                Byte[] bbtextra = BitConverter.GetBytes(table_extra);
                int hsz = 8;//header.get_head_size();
                int sz = 24;// header.get_body_size();

                Byte[] header = null;//base.encode();
                Byte[] arr = new Byte[sz];
                header.CopyTo(arr, 0);
                bbuserid.CopyTo(arr, hsz);
                bbtnum.CopyTo(arr, hsz + bbuserid.Length);
                bbtextra.CopyTo(arr, hsz + bbuserid.Length + bbtnum.Length);
                return arr;
            }
            override public void decode()
            {
                
            }
            public int bind_userid = 0;
            public int table_num = 0;
            public ushort table_extra = 0;

        }
        class ReadyReq:IMsg 
        {
            public ReadyReq()
            {
                type_ = 0xA9;
                size_ = 10;

            }
            override public void process()
            {

            }
            public override IMsg create_obj()
            {
                return new ReadyReq();
            }

            override public Byte[] encode()
            {
                Byte[] brtype = BitConverter.GetBytes(ready_type);
                int hsz = 8;//header.get_head_size();
                int sz = 10;

                Byte[] header = null;// header.encode();
                Byte[] arr = new Byte[sz];
                header.CopyTo(arr, 0);
                brtype.CopyTo(arr, hsz);
                return arr;
            }
            override public void decode()
            {
                
            }
            public short ready_type = 0;

        }
    }
}
