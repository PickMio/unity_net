using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
   abstract class IHeader
   {
       public enum MSG_STATUS{           
           MSG_STATUS_HEAD,
           MSG_STATUS_BODY,
       };

       abstract public Byte[] encode();
       abstract public void decode();
       public Byte[] message
       {
           set
           {
               msg_ = (Byte[])value.Clone();
           }
           get
           {
               return msg_;
           }
       }
       private Byte[] msg_;
       protected int head_size_;

       public int head_size
       {
           get { return head_size_; }
       }
       private int body_size_;

       public int body_size
       {
           get { return body_size_; }
           set { body_size_ = value; }
       }
       private int message_type_;

       public int message_type
       {
           get { return message_type_; }
           set { message_type_ = value; }
       }
       public MSG_STATUS status_ = MSG_STATUS.MSG_STATUS_HEAD;

   }
    abstract class IMsg
    {
        public IMsg(){}

        //自动创建对象
        abstract public IMsg create_obj();
        //所有的类型消息都应该继承自该接口, 
        abstract public Byte[] encode();
        abstract public void decode();
        //服务器处理消息
        abstract public void process();
        //消息数据


        
        public Byte[] message
        {
            set
            {
                msg_ = (Byte[])value.Clone();
            }

        }
        public int type 
        {
            get
            {
                return type_;
            }

        }
        public int size 
        {
            get
            {
                return size_;
            }

        }


        private Byte[] msg_;
        //每个消息都有个默认类型值, 构造函数式赋值
        protected int type_;
        protected int size_;
    }

    /// <summary>
    /// 系统消息之未知消息
    /// </summary>
    class SysUnkonwnMessage : IMsg 
    {
        public SysUnkonwnMessage()
        {
            type_ = 0xFFF1;
        }
        override public Byte[] encode()
        {
            return null;

        }
        override public void decode()
        {
        }

        override public IMsg create_obj()
        {
            return new SysUnkonwnMessage();
        }
        //服务器处理消息
        override public void process()
        {
            Logger.log("unkonwn message, ");

        }


    }
    class SysSocketCloseMessage : IMsg 
    {
        public SysSocketCloseMessage()
        {
            type_ = 0xFFF2;
        }
        override public Byte[] encode()
        {
            return null;

        }
        override public void decode()
        {
        }

        override public IMsg create_obj()
        {
            return new SysSocketCloseMessage();
        }
        //服务器处理消息
        override public void process()
        {
            Logger.log(" client close message ");

        }


    }
}
