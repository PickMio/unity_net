using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace net_work
{
    public class DdzNetMsg
    {
        public class Header : IMsg
        {
            public short type
            {
                get
                {
                    return type_;
                }
                set
                {
                    type_ = value;
                }
            }
            public short length 
            {
                get
                {
                    return length_;
                }
                set
                {
                    length_ = value;
                }
            }
            public Header()
            {
                identity_ = 0x05;
                encode_ = 0x00;
                length_ = 8;
                version_   = 0x03;
                reserve_ = 0x00;
                type_ = 0x00;
            }
            private byte identity_;
            private byte encode_;
            private short length_;
            private byte version_;
            private byte reserve_;
            private short type_;
            public override Byte[] encode()
            {


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
            override public IMsg decode()
            {
                return null;

            }
        }
        public class Authen_Req: Header
        {
            public Authen_Req()
            {
                type = 0xA0;
            }
            override public Byte[] encode()
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

                int hsz = base.length;
                //int sz = base.length + buserid.Length + broomid.Length + bpasswd.Length + blogin_type.Length;
                int sz = 56;
                base.length = (short)sz;

                Byte[] header = base.encode();
                Byte[] arr = new Byte[sz];
                header.CopyTo(arr, 0);
                buserid.CopyTo(arr, hsz);
                broomid.CopyTo(arr, hsz + buserid.Length);
                bpasswd.CopyTo(arr, hsz + buserid.Length + broomid.Length);
                blogin_type.CopyTo(arr, 52);

                return arr;
            }
            override public IMsg decode()
            {
                return null;

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
        public class SitdownReq: Header
        {
            public SitdownReq()
            {
                type = 0xA4;
            }

            override public Byte[] encode()
            {
                Byte[] bbuserid = BitConverter.GetBytes(bind_userid);
                Byte[] bbtnum = BitConverter.GetBytes(table_num);
                Byte[] bbtextra = BitConverter.GetBytes(table_extra);
                int hsz = base.length;
                int sz = 24;
                base.length = (short)sz;

                Byte[] header = base.encode();
                Byte[] arr = new Byte[sz];
                header.CopyTo(arr, 0);
                bbuserid.CopyTo(arr, hsz);
                bbtnum.CopyTo(arr, hsz + bbuserid.Length);
                bbtextra.CopyTo(arr, hsz + bbuserid.Length + bbtnum.Length);
                return arr;
            }
            override public IMsg decode()
            {
                return null;
            }
            public int bind_userid = 0;
            public int table_num = 0;
            public ushort table_extra = 0;

        }
        public class ReadyReq: Header
        {
            public ReadyReq()
            {
                type = 0xA9;
            }

            override public Byte[] encode()
            {
                Byte[] brtype = BitConverter.GetBytes(ready_type);
                int hsz = base.length;
                int sz = 10;
                base.length = (short)sz;

                Byte[] header = base.encode();
                Byte[] arr = new Byte[sz];
                header.CopyTo(arr, 0);
                brtype.CopyTo(arr, hsz);
                return arr;
            }
            override public IMsg decode()
            {
                return null;
            }
            public short ready_type = 0;

        }
    }
}
