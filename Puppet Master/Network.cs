using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Network
    {
        private SysConfig sysConfig = new SysConfig();

        public Network()
        {
            this.sysConfig.Semantics = SysConfig.AT_MOST_ONCE;
            this.sysConfig.LoggingLevel = SysConfig.LIGHT;
            this.sysConfig.Routing = SysConfig.PRIMARY;
        }

        public SysConfig SysConfig
        {
            get
            {
                return this.sysConfig;
            }
        }

        public string Semantics
        {
            get
            {
                return this.sysConfig.Semantics;
            }

            set
            {
                this.sysConfig.Semantics = value;
            }
        }

        public string LoggingLevel
        {
            get
            {
                return this.sysConfig.LoggingLevel;
            }

            set
            {
                this.sysConfig.LoggingLevel = value;
            }
        }

        public string Routing
        {
            get
            {
                return this.sysConfig.Routing;
            }

            set
            {
                this.sysConfig.LoggingLevel = value;
            }
        }
    }
}
