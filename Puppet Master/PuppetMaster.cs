using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Shared_Library;
using System.Runtime.Remoting;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            PuppetMaster pm = new PuppetMaster();
            pm.start();
        }
    }

    class PuppetMaster : MarshalByRefObject, IRemotePuppetMaster
    {
        private static String CONFIG_FILE_PATH = @"../../Config.txt";
        private static String SCRIPT_FILE_PATH = @"../../Script.txt";

        public void start()
        {
            Console.WriteLine("Registering Puppet Master...");
            registerPM();
            Console.WriteLine("Reading configuration file...");
            readConfig();
            Console.WriteLine("Processing script file...");
            readScript();
            Console.WriteLine("Waiting input (exit to finish)");
            manualMode();
            Console.WriteLine("Shutingdown the network...");
            shutDownAll();
            Console.WriteLine("All processes was terminated.");

        }

        private void registerPM()
        {
            TcpChannel channel = new TcpChannel(SysConfig.PM_PORT);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, SysConfig.PM_NAME, typeof(IRemotePuppetMaster));
        }

        private void readConfig()
        {
        }

        private void readScript()
        {
        }

        private void manualMode()
        {
        }

        private void shutDownAll()
        {
        }

        #region "Interface Methods"
        public void registerLog()
        {

        }

        #endregion

    }
}
