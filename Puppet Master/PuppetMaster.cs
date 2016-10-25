using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Shared_Library;
using System.Runtime.Remoting;
using System.IO;


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

        private SysConfig sysConfig = new SysConfig();

        public PuppetMaster()
        {
            sysConfig.Semantics = SysConfig.AT_MOST_ONCE;
            sysConfig.LoggingLevel = SysConfig.LIGHT;
            sysConfig.Routing = SysConfig.PRIMARY;
        }
       
        public void start()
        {
            Console.WriteLine("Registering Puppet Master...");
            registerPM();
            Console.WriteLine("Reading configuration file...");
            readConfig();
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
            string line;
            int lineNr = 0;
            StreamReader file;

            file = new StreamReader(CONFIG_FILE_PATH);

            while ((line = file.ReadLine()) != null)
                doConfigLine(line, lineNr++);

            Console.WriteLine("Successfully parsed the configuration file");
        }

        private void doConfigLine(String line, int lineNr)
        {
            string[] lineArray = line.Split(' ');
            string option = lineArray[0].ToLower();

            if (option == "semantics")
            {
                doSemanticsCommand(lineArray, lineNr);
            }
            else if (option == "logginglevel")
            {
                doLoggingLevelCommand(lineArray, lineNr);
            }
            else if (option == "start")
            {
                doStartCommand(lineArray, lineNr);
            }
            else if (option == "status")
            {
                doStatusCommand(lineArray, lineNr);
            }
            else if (option == "interval")
            {
                doIntervalCommand(lineArray, lineNr);
            }
            else if (option == "crash")
            {
                doCrashCommand(lineArray, lineNr);
            }
            else if (option == "freeze")
            {
                doFreezeCommand(lineArray, lineNr);
            }
            else if (option == "unfreeze")
            {
                doUnfreezeCommand(lineArray, lineNr);
            }
            else if (option == "wait")
            {
                doWaitCommand(lineArray, lineNr);
            }
            else if (option.StartsWith("op"))
            {
                doOperatorCommand(lineArray, lineNr);
            }
            else if (option.StartsWith("%"))
            {
                // do nothing, it's a comment
            }
            else
            {
                Console.WriteLine("The command at line " + lineNr + " doesn't exist:" + line);
            }
        }

        private void doSemanticsCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Semantics at-most-once | at-least-once | exactly-once");

            if (line[1].ToLower() == SysConfig.AT_LEAST_ONCE ||
                line[1].ToLower() == SysConfig.AT_MOST_ONCE ||
                line[1].ToLower() == SysConfig.EXACTLY_ONCE)
            {
                sysConfig.Semantics = line[1].ToLower();
            }
            else
            {
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The valid options for semantics are: at-most-once | at-least-once | exactly-once.");
            }
                
        }

        private void doLoggingLevelCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is LoggingLevel light | full");

            if (line[1].ToLower() == SysConfig.LIGHT ||
                line[1].ToLower() == SysConfig.FULL)
            {
                sysConfig.LoggingLevel = line[1].ToLower();
            }
            else
            {
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The valid options for LoggingLevel are: light | full.");
            }
        }

        private void doStartCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Start operator_id");

            // START COMMAND
        }

        private void doStatusCommand(string[] line, int lineNr)
        {
            if (line.Length != 1)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Start operator_id");

            // STATUS COMMAND
        }

        private void doIntervalCommand(string[] line, int lineNr)
        {
            int ms;

            if (line.Length != 3)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Interval operator_id x_ms");

            if (Int32.TryParse(line[2], out ms))
            {
                // INTERVAL COMMAND 
            }
            else
            {
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". You must insert a valid number.");
            }

        }

        private void doCrashCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Crash process_name");

            // CRASH COMMAND
        }

        private void doFreezeCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Freeze process_name");

            // FREEZE COMMAND
        }

        private void doUnfreezeCommand(string[] line, int lineNr)
        {
            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Unfreeze process_name");

            // UNFREEZE COMMAND
        }

        private void doWaitCommand(string[] line, int lineNr)
        {
            int ms; 

            if (line.Length != 2)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Wait x_ms");

            if (Int32.TryParse(line[2], out ms))
            {
                // WAIT COMMAND 
            }
            else
            {
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". You must insert a valid number.");
            }
        }

        private void doOperatorCommand(string[] line, int lineNr)
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
