using System;

namespace PuppetMaster
{
    public class ParseException : Exception
    {
        private string msg;

        public ParseException(String message) : base(message) { msg = message; }

        public ParseException(String message, Exception inner) : base(message, inner) { msg = message; }

        public string Msg { 
            get{ return msg; }
            set{ msg = value;  }
        }

    }
}
