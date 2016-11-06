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

    public class WrongOpSpecsException : Exception
    {
        public WrongOpSpecsException() { }

        public WrongOpSpecsException(String message) : base(message) { }

        public WrongOpSpecsException(String message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class OpByteCodesNotReceivedException : ApplicationException
    {
        public IRemoteOperator iOp;

        public OpByteCodesNotReceivedException(IRemoteOperator iOp)
        {
            this.iOp = iOp;
        }

        public OpByteCodesNotReceivedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            iOp = (IRemoteOperator)info.GetValue("iOp", typeof(IRemoteOperator));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("iOp", iOp);
        }
    }
      
}
