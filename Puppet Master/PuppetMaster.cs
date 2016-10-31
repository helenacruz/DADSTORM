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
        private static String CONFIG_FILE_PATH = @"../../../Config.txt";
        private static String SCRIPT_FILE_PATH = @"../../../Script.txt";
        private string pmurl;

        private SysConfig sysConfig;

        private Dictionary<int, List<string>> operators_addresses;
        private Dictionary<int, IRemoteOperator> operators;

        TcpChannel channel;

        public PuppetMaster()
        {
            sysConfig = new SysConfig();
            operators_addresses = new Dictionary<int, List<string>>();
            operators = new Dictionary<int, IRemoteOperator>();


            sysConfig.Semantics = SysConfig.AT_MOST_ONCE;
            sysConfig.LoggingLevel = SysConfig.LIGHT;

        }
       
        public void start()
        {
            Console.WriteLine("Registering Puppet Master...");
            registerPM();
            Console.WriteLine("Reading configuration file...");
            readCommands(CONFIG_FILE_PATH);
            Console.WriteLine("Reading script file...");
            readCommands(SCRIPT_FILE_PATH);
            Console.WriteLine("Waiting input (exit to finish)");
            manualMode();
            Console.WriteLine("Shutingdown the network...");
            shutDownAll();
            Console.WriteLine("All processes was terminated.");

        }

        private void registerPM()
        {
            channel = new TcpChannel(SysConfig.PM_PORT);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, SysConfig.PM_NAME, typeof(IRemotePuppetMaster));
            pmurl = SysConfig.PM_URL;
        }

        private void readCommands(string path)
        {
            string line;
            int lineNr = 0;
            StreamReader file;

            file = new StreamReader(path);

            while ((line = file.ReadLine()) != null)
                doCommandLine(line, lineNr++);

            Console.WriteLine("Successfully parsed the file.");
        }

        private void doCommandLine(String line, int lineNr)
        {
            string[] lineArray = line.Split(' ');
            string option = lineArray[0].ToLower();
            if (line != "")
            {
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
                    Console.WriteLine("The configuration command at line " + lineNr + " doesn't exist:" + line);
                }
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

            int opId;
            if (!Int32.TryParse(line[1].ToLower().Replace("op", ""), out opId))
            {
                throw new ParseException("Invalid operator id. Must be OPn, where n is an integer.");
            }
            if (opId < operators.Count)
            {
                IRemoteOperator op = operators[opId];
                op.startOperator();
            }
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
            if (line.Length != 3)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Crash process_name");

            // CRASH COMMAND
        }

        private void doFreezeCommand(string[] line, int lineNr)
        {
            if (line.Length != 3)
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Freeze process_name");

            // FREEZE COMMAND
        }

        private void doUnfreezeCommand(string[] line, int lineNr)
        {
            if (line.Length != 3)
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

            if (Int32.TryParse(line[1], out ms))
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
            int rep_fact=1;
            int op=-1;
            string routing = null;
            List<string> sources=null;
            List<string> addresses=null;
            List<string> opSpecs=null;
            string type = null;

            if (!Int32.TryParse(line[0].ToLower().Replace("op", ""), out op))
            {
                throw new ParseException("Invalid operator id. Must be OPn, where n is an integer.");
            }
            
            for (i = 0; i < len; i++)
            {
                if (line[i].ToLower() == "input_ops")
                {
                    sources = doInputOps(line, i);
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
                    if (line[i + 1].Contains(",") && line[i+1].Split(',').Length != rep_fact)
                        throw new ParseException("Invalid number of addresses at line " + lineNr + ". It must be "+rep_fact+".");
                    addresses = line[i+1].Split(',').ToList();
                    if (operators_addresses.ContainsKey(op))
                        throw new ParseException("The operator " + op + " at line " + lineNr + " already exists.");
                    operators_addresses.Add(op, addresses);
                }
                if (line[i].ToLower() == "operator_spec")
                {
                    type = line[i + 1];
                    if (line[i + 1].Contains(","))
                        opSpecs = line[i + 2].Split(',').ToList();
                    else
                    {
                        List<string> aux = new List<String>();
                        if (i + 2 < line.Length)
                            aux.Add(line[i + 2]);
                        else
                            aux = null;
                        opSpecs = aux;
                    }
                }
            }

            string[] aux1 = addresses[0].Split('/');
            string address = aux1[aux1.Length-2];
            string[] aux2 = address.Split(':');

            string pcs_address = aux1[0];
            for(int j = 1; j < aux1.Length-1; j++)
            {
                if(j==aux1.Length-2)
                    pcs_address += "/" + aux2[0]+":"+SysConfig.PCS_PORT + "/" + SysConfig.PCS_NAME;
                else
                    pcs_address += "/" + aux1[j];
            }

            doCreateOperator(pmurl, ""+op, sources, ""+rep_fact, routing, addresses,pcs_address, Int32.Parse(aux2[1]), type, opSpecs);
        }

        private void doCreateOperator(string pmurl, string id, List<string> sources, String rep_fact, String routing, List<String> urls,string pcs_address, int port, string type, List<string> op_specs)
        {
            
            IRemoteProcessCreation pcs = (IRemoteProcessCreation)Activator.GetObject(typeof(IRemoteProcessCreation), pcs_address);
            if (pcs == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + pcs_address);
            pcs.createOP(pmurl, id, sources, rep_fact, routing, urls, port, type, op_specs);

            IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(IRemoteOperator), urls[0]);
            if (op == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + urls[0]);
            operators.Add(Int32.Parse(id), op);
        }

        private List<string> doInputOps(String[] line, int i)
        {
            i++; // i is "sources"

            List<string> sources = new List<string>();
            List<string> sources_processed = new List<string>();
            sources = line[i ].Split(',').ToList();
            foreach (string s in sources)
            {
                if (s.ToLower().Contains("op"))
                {
                    int op;
                    if (!Int32.TryParse(s.ToLower().Replace("op", ""), out op))
                    {
                        throw new ParseException("Invalid operator id. Must be OPn, where n is an integer.");
                    }
                    sources_processed.Add(operators_addresses[op][0]);
                }
                else
                    sources_processed.Add(s);
            }
            return sources_processed;
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
