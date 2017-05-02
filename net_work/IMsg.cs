using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_work
{
    abstract class IMsg
    {
        //所有的类型消息都应该继承自该接口, 
        abstract public Byte[] encode();
        abstract public IMsg decode();

        public Byte[] message
        {
            set
            {
                msg_ = (Byte[])value.Clone();
            }

        }
        private Byte[] msg_;
    }
}
