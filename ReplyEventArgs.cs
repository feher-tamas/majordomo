using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Majordomo.Common
{
    public class ReplyEventArgs : EventArgs
    {
        public NetMQMessage Reply { get; private set; }

        public Exception Exception { get; private set; }

        public ReplyEventArgs(NetMQMessage reply)
        {
            Reply = reply;
        }

        public ReplyEventArgs(NetMQMessage reply, Exception exception)
            : this(reply)
        {
            Exception = exception;
        }

        public bool HasError()
        {
            return (Exception != null ? true : false);
        }

    }
}
