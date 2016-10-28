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
}
