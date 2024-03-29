﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{
    [Serializable]
    public class SysConfig
    {
        public const int PM_PORT = 10000;
        public const string PM_NAME = "puppet";
        public const string PM_URL = "tcp://localhost:10000/puppet";

        public const int PCS_PORT = 10001;
        public const string PCS_NAME = "pcreation";

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

        // Commands
        public const string UNIQUE = "uniq";
        public const string COUNT = "count";
        public const string DUP = "dup";
        public const string FILTER = "filter";
        public const string CUSTOM = "custom";

        private string semantics;
        private string loggingLevel;

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

    }
}
