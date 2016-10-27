﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class CannotAccessRemoteObjectException : Exception
    {
        public CannotAccessRemoteObjectException() { }

        public CannotAccessRemoteObjectException(String message) : base(message) { }

        public CannotAccessRemoteObjectException(String message, Exception inner) : base(message, inner) { }
    }

    class OperatorSpecsException : Exception
    {
        public OperatorSpecsException() { }

        public OperatorSpecsException(String message) : base(message) { }

        public OperatorSpecsException(String message, Exception inner) : base(message, inner) { }

    }
}
