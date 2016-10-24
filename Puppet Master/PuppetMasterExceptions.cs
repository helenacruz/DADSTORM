using System;

namespace PuppetMaster
{
    public class ParseException : Exception
    {
        public ParseException() { }

        public ParseException(String message) : base(message) { }

        public ParseException(String message, Exception inner) : base(message, inner) { }

    }
}
