﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreation
{
    class OperatorSpecsException : Exception
    {
        public OperatorSpecsException() { }

        public OperatorSpecsException(String message) : base(message) { }

        public OperatorSpecsException(String message, Exception inner) : base(message, inner) { }

    }
}
