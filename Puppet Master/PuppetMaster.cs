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

        private SysConfig sysConfig;

        private Dictionary<int, List<string>> operators;

        public PuppetMaster()
        {
            sysConfig = new SysConfig();
            operators = new Dictionary<int, List<string>>();

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
            if (line.Length < 7)
                throw new ParseException("Too few arguments to OP command. Usage: " +
                   "operator_id INPUT_OPS source_op_id1|filepath1,...,source_op_idn|filepathn " +
                   "REP_FACT repl_factor ROUTING primary | hashing(n) | random " +
                   "ADDRESS URL1, ..., URLrepl_fact " +
                   "OPERATOR_SPEC operator_type operator_param_1, ..., operator_param_n");

            int i;
            int len = line.Length;
            int rep_fact;
            int op;
            string routing;
            List<string> sources;
            List<string> addresses;
            Dictionary<string, List<string>> opSpec;

            if (!Int32.TryParse(line[0].ToLower().Replace("op", ""), out op))
            {
                throw new ParseException("Invalid operator id. Must be OPn, where n is an integer.");
            }
            
            for (i = 0; i < len; i++)
            {
                if (line[i].ToLower() == "input_ops")
                {
                    sources = doSources(line, i);
                }
                if (line[i].ToLower() == "rep_fact")
                {
                    rep_fact = doRepFact(line, i);
                }
                if (line[i].ToLower() == "routing")
                {
                    routing = doRouting(line, i);
                }
                if (line[i].ToLower() == "address")
                {
                    addresses = doAddresses(line, i);
                    operators.Add(op, addresses);
                }
                if (line[i].ToLower() == "operator_spec")
                {
                    opSpec = doOperatorSpec(line, i);
                }
            }

            // create operator
        }

        private List<string> doSources(String[] line, int i)
        {
            int len;
            List<string> sources = new List<string>();

            i++; // i is "input_ops"

            for (len = line.Length; i < len; i++)
            {
                if (line[i].ToLower() == "rep_fact" || line[i].ToLower() == "routing" ||
                    line[i].ToLower() == "address" || line[i].ToLower() == "operator_spec")
                {
                    break;
                }
                sources.Add(line[i]);
            }

            return sources;
        }

        private int doRepFact(String[] line, int i)
        {
            int len;
            int rep_factor;

            i++; // i is "rep_factor"

            for (len = line.Length; i < len; i++)
            {
                if (line[i].ToLower() == "address" || line[i].ToLower() == "routing" ||
                    line[i].ToLower() == "input_ops" || line[i].ToLower() == "operator_spec")
                {
                    break;
                }
                if (Int32.TryParse(line[i], out rep_factor))
                {
                    return rep_factor;
                }
            }

            throw new ParseException("Rep_factor");
        }

        private string doRouting(String[] line, int i)
        {
            int len;
            string routing;

            i++; // i is "routing"

            for (len = line.Length; i < len; i++)
            {
                if (line[i].ToLower() == "rep_fact" || line[i].ToLower() == "input_ops" ||
                    line[i].ToLower() == "address" || line[i].ToLower() == "operator_spec")
                {
                    break;
                }
                routing = line[i].ToLower();
                return routing;
            }

            throw new ParseException("Routing");
        }

        private List<string> doAddresses(String[] line, int i)
        {
            int len;
            List<string> addresses = new List<string>();

            i++; // i is "adress"

            for (len = line.Length; i < len; i++)
            {
                if (line[i].ToLower() == "rep_fact" || line[i].ToLower() == "input_ops" ||
                    line[i].ToLower() == "routing" || line[i].ToLower() == "operator_spec")
                {
                    break;
                }
                addresses.Add(line[i]);
            }

            return addresses;
        }

        private Dictionary<string, List<string>> doOperatorSpec(String[] line, int i)
        {
            int len;
            string operatorType;
            List<string> operatorParams = new List<string>();
            Dictionary<string, List<string>> op = new Dictionary<string, List<string>>();

            i++; // i is "adress"

            operatorType = line[i];

            i++;

            for (len = line.Length; i < len; i++)
            {
                if (line[i].ToLower() == "rep_fact" || line[i].ToLower() == "input_ops" ||
                    line[i].ToLower() == "routing" || line[i].ToLower() == "address")
                {
                    break;
                }

                operatorParams.Add(line[i].ToLower());
            }

            op.Add(operatorType, operatorParams);

            return op;
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
