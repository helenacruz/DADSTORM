using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{
    public class CannotAccessRemoteObjectException : Exception
    {
        public CannotAccessRemoteObjectException() { }

        public CannotAccessRemoteObjectException(String message) : base(message) { }

        public CannotAccessRemoteObjectException(String message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class WrongOpSpecsException : ApplicationException
    {
        public string msg;

        public WrongOpSpecsException(String msg)
        {
            this.msg = msg;
        }

        public WrongOpSpecsException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            msg = (string)info.GetValue("msg", typeof(string));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("msg", msg);
        }
    }

    [Serializable]
    public class OpByteCodesNotReceivedException : ApplicationException
    {
        public string  msg;

        public OpByteCodesNotReceivedException(string msg)
        {
            this.msg = msg;
        }

        public OpByteCodesNotReceivedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            msg = (string)info.GetValue("msg", typeof(string));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("msg", msg);
        }
    }
      
}
