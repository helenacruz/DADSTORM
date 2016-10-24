using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{
    public class SysConfig
    {
        public const int PM_PORT = 10000;
        public const string PM_NAME = "puppet";

        // Semantics
        public const string AT_MOST_ONCE = "at-most-once";
        public const string AT_LEAST_ONCE = "at-least-once";
        public const string EXACTLY_ONCE = "exactly-once";

        // Logging level
        public const string LIGHT = "light";
        public const string FULL = "full";

        // Routing
        public const string PRIMARY = "primary";
        public const string RANDOM = "random";
        public const string HASHING = "hashing";

        private string semantics;
        private string loggingLevel;
        private string routing;

        public string Semantics
        {
            get
            {
                return semantics;
            }

            set
            {
                semantics = value;
            }
        }

        public string LoggingLevel
        {
            get
            {
                return loggingLevel;
            }

            set
            {
                loggingLevel = value;
            }
        }

        public string Routing
        {
            get
            {
                return routing;
            }

            set
            {
                routing = value;
            }
        }

    }
}
