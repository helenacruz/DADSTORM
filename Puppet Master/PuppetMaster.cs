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
using System.Windows.Forms;

namespace PuppetMaster
{
    public class UpdateLogsArgs : EventArgs
    {
        public string log;

        public UpdateLogsArgs(string log)
        {
            this.log = log;
        }
    }

    class PuppetMaster : MarshalByRefObject, IRemotePuppetMaster
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new PuppetMasterUI();
            Application.Run(form);
        }

        private static PuppetMasterUI form;

        public delegate void RemoteAsyncCreateOpDelegate(string primary,string opName,string port,SysConfig sysConfig, string pmurl, IList<IList<string>> sources, String rep_fact, String routing, IList<String> urls);
        public delegate void RemoteAsyncSendOpDelegate(byte[] code, string className, string method, IList<string> op_specs);
        public delegate void RemoteAsyncNoArgsOpDelegate();
        public delegate void RemoteAsyncIntervalOpDelegate(int ms);

        private static String CONFIG_FILE_PATH = @"../../../Config.txt";
        private static String SCRIPT_FILE_PATH = @"../../../Script.txt";
        private static String LIB_OPERATORS_PATH = @"../../../LibOperator/bin/Debug/LibOperator.dll";
        private static String DEFAULT_METHOD = "CustomOperation";

        private string pmurl;

        private SysConfig sysConfig;

        private Dictionary<int, IList<string>> operators_addresses;
        private Dictionary<int, IList<IRemoteOperator>> operators;

        TcpChannel channel;

        bool manualM = false;

        private List<string> waitListCommands;
        private List<string> logs;
        bool finishedScript = false;

        public PuppetMaster()
        {
            sysConfig = new SysConfig();
            operators_addresses = new Dictionary<int, IList<string>>();
            operators = new Dictionary<int, IList<IRemoteOperator>>();
            waitListCommands = new List<string>();
            logs = new List<string>();

            sysConfig.Semantics = SysConfig.AT_MOST_ONCE;
            sysConfig.LoggingLevel = SysConfig.LIGHT;

            addCommandsToWaitList(SCRIPT_FILE_PATH);
        }

        private void logInfo(string log)
        {
            object sender = System.Threading.Thread.CurrentThread;
            UpdateLogsArgs e;

            if (form != null)
            {
                if (logs.Count > 0)
                {
                    foreach (string pendingLog in logs)
                    {
                        e = new UpdateLogsArgs(pendingLog);
                        form.updateLogsUI(sender, e);
                    }
                    logs.Clear();
                }
                e = new UpdateLogsArgs(log);
                form.updateLogsUI(sender, e);
            }
            else
            {
                logs.Add(log);
            }
        }

        public string getLogs()
        {
            string result = "";

            foreach (string log in logs)
            {
                result += log + "\r\n";
            }

            if (result.Length > 0)
            {
                result.Remove(result.Length - 1);
            }

            return result;
        }

        public bool finishedparsingScript()
        {
            return finishedScript;
        }          

        public void readScriptFile()
        {
            string log = "Reading script file...";
            Console.WriteLine(log);
            logInfo(log);
            readCommands(SCRIPT_FILE_PATH);
        }
        
        public void processOneMoreStep()
        {
            if (waitListCommands.Count > 0)
            {
                string command = waitListCommands[0];
                waitListCommands.RemoveAt(0);
                enterCommand(command);
            }
            else
            {
                finishedScript = true;
            }
        }

        public void start()
        {
            string log = "Registering Puppet Master...";
            Console.WriteLine(log);
            logInfo(log);

            registerPM();

            log = "Reading configuration file...";
            Console.WriteLine(log);
            logInfo(log);

            readCommands(CONFIG_FILE_PATH);
            
            IList<IRemoteOperator> lastReplica = operators[operators.Count];
            foreach (IRemoteOperator op in lastReplica)
            {
                RemoteAsyncNoArgsOpDelegate RemoteSendDel = new RemoteAsyncNoArgsOpDelegate(op.makeAsOutputOperator);
                IAsyncResult RemSendAr = RemoteSendDel.BeginInvoke(null, null);
            }
           
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
            {
                try
                {
                    doCommandLine(line, lineNr++);
                }
                catch (ParseException e)
                {
                    Console.WriteLine(e.Msg);
                }

            }

        }

        private void addCommandsToWaitList(string path)
        {
            string line;
            StreamReader file;

            file = new StreamReader(path);

            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("%") && !lineIsEmpty(line))
                {
                    waitListCommands.Add(line);
                    Console.WriteLine("***" + line + "***");
                }
            }
        }

        private bool lineIsEmpty(string line)
        {
            if (line.StartsWith(Environment.NewLine))
            {
                return true;
            }
            if (line.StartsWith(" "))
            {
                return true;
            }
            if (line == "")
            {
                return true;
            }

            return false;
        }

        private void doCommandLine(String line, int lineNr)
        {
            string[] lineArray = line.Split(' ');
            string option = lineArray[0].ToLower();

            if (line != "")
            {
                if (option == "semantics")
                {
                    logInfo(line);
                    doSemanticsCommand(lineArray, lineNr);
                }
                else if (option == "logginglevel")
                {
                    logInfo(line);
                    doLoggingLevelCommand(lineArray, lineNr);
                }
                else if (option == "start")
                {
                    logInfo(line);
                    doStartCommand(lineArray, lineNr);
                    if(!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "status")
                {
                    logInfo(line);
                    doStatusCommand(lineArray, lineNr);
                    if (!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "interval")
                {
                    logInfo(line);
                    doIntervalCommand(lineArray, lineNr);
                    if (!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "crash")
                {
                    logInfo(line);
                    doCrashCommand(lineArray, lineNr);
                    if (!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "freeze")
                {
                    logInfo(line);
                    doFreezeCommand(lineArray, lineNr);
                    if (!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "unfreeze")
                {
                    logInfo(line);
                    doUnfreezeCommand(lineArray, lineNr);
                    if (!manualM)
                        Console.WriteLine(line);
                }
                else if (option == "wait")
                {
                    logInfo(line);
                    doWaitCommand(lineArray, lineNr);
                }
                else if (option.StartsWith("op"))
                {
                    logInfo(line);
                    doOperatorCommand(lineArray, lineNr);
                }
                else if (option.StartsWith("%"))
                {
                    // do nothing, it's a comment
                }
                else
                {
                    string log = "The configuration command at line " + lineNr + " doesn't exist:" + line;
                    Console.WriteLine(log);
                    logInfo(log);
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
            if (opId <= operators.Count)
            {
                IList<IRemoteOperator> ops = operators[opId];
                foreach (IRemoteOperator op in ops)
                {
                    RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.startOperator);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
                }
                 
            }
        }

        private void doStatusCommand(string[] line, int lineNr)
        {
            foreach (KeyValuePair<int, IList<IRemoteOperator>> entry in operators)
            {

                IList<IRemoteOperator> ops = operators[entry.Key];
                foreach (IRemoteOperator op in ops)
                {
                    RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.status);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
                }
            }
        }

        private void doIntervalCommand(string[] line, int lineNr)
        {
            int ms;
            int opId = -1;

            if (line.Length != 3 || !Int32.TryParse(line[1].ToLower().Replace("op", ""), out opId) || !Int32.TryParse(line[2], out ms))
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Interval operator_id x_ms");

            IList<IRemoteOperator> ops = operators[opId];
            foreach (IRemoteOperator op in ops)
            {
                RemoteAsyncIntervalOpDelegate RemoteDel = new RemoteAsyncIntervalOpDelegate(op.interval);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(ms,null, null);
            }
           
        }

        private void doCrashCommand(string[] line, int lineNr)
        {
            int opId = -1;
            int opReplica = -1;

            if (line.Length != 3 || !Int32.TryParse(line[1].ToLower().Replace("op", ""), out opId) || !Int32.TryParse(line[2], out opReplica))
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Crash process_name replica_number");

            if(opId<=operators.Count && opReplica<= operators[opId].Count - 1)
            {
                IRemoteOperator op = operators[opId][opReplica];
                RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.crash);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
            }
            else
                throw new ParseException("Error parsing file in line " + lineNr +
               ". Wrong process_name or replica_number");
        }

        private void doFreezeCommand(string[] line, int lineNr)
        {
            int opId = -1;
            int opReplica = -1;

            if (line.Length != 3 || !Int32.TryParse(line[1].ToLower().Replace("op", ""), out opId) || !Int32.TryParse(line[2], out opReplica))
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Crash process_name replica_number");
            IRemoteOperator op = operators[opId][opReplica];
            RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.freeze);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
        }

        private void doUnfreezeCommand(string[] line, int lineNr)
        {
            int opId = -1;
            int opReplica = -1;

            if (line.Length != 3 || !Int32.TryParse(line[1].ToLower().Replace("op", ""), out opId) || !Int32.TryParse(line[2], out opReplica))
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Crash process_name replica_number");

            IRemoteOperator op = operators[opId][opReplica];
            RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.unfreeze);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
        }

        private void doWaitCommand(string[] line, int lineNr)
        {
            int ms; 

            if (line.Length != 2 || !Int32.TryParse(line[1], out ms))
                throw new ParseException("Error parsing file in line " + lineNr +
                    ". The correct format is Wait x_ms");
            if (!manualM)
                 Console.WriteLine(line[0]+" "+line[1]);

            System.Threading.Thread.Sleep(ms);
            
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
            IList<IList<string>> sources=null;
            IList<string> addresses=null;
            IList<string> opSpecs=null;
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
                    if (i + 2 == line.Length)
                        opSpecs = null;
                    else if (line[i + 2].Contains(","))
                    {
                        opSpecs = line[i + 2].Split(',').ToList();
                    }
                    else
                    {
                        List<string> aux = new List<String>();
                        aux.Add(line[i + 2]);
                        opSpecs = aux;
                    }
                }
            }


            doCreateOperator(pmurl, ""+op, sources, ""+rep_fact, routing, addresses, type, opSpecs);
        }

        private void doCreateOperator(string pmurl,  string id, IList<IList<string>> sources, String rep_fact, String routing, IList<String> urls, string type, IList<string> op_specs)
        {
            IList<IRemoteOperator> replicas = new List<IRemoteOperator>();
            string primary = "true";

            foreach (string url in urls)
            {
                string[] aux1 = url.Split('/');
                string address = aux1[aux1.Length - 2];
                string nameOp = aux1[aux1.Length - 1];

                string[] aux2 = address.Split(':');

                string pcs_address = aux1[0];
                string port = aux2[1];

                for (int j = 1; j < aux1.Length - 1; j++)
                {
                    if (j == aux1.Length - 2)
                        pcs_address += "/" + aux2[0] + ":" + SysConfig.PCS_PORT + "/" + SysConfig.PCS_NAME;
                    else
                        pcs_address += "/" + aux1[j];
                }

                IRemoteProcessCreation pcs = (IRemoteProcessCreation)Activator.GetObject(typeof(IRemoteProcessCreation), pcs_address);
                if (pcs == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + pcs_address);

                //first url is of machine and others are from replicas
                IList<string> orderedUrls = new List<string>();
                orderedUrls.Add(url);
                foreach (string s in urls)
                {
                    if(!s.Equals(url))
                        orderedUrls.Add(s);
                }

                RemoteAsyncCreateOpDelegate RemoteDel = new RemoteAsyncCreateOpDelegate(pcs.createOP);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(primary,nameOp,port, sysConfig, pmurl, sources, rep_fact, routing, orderedUrls, null, null);
                primary = "false";

                IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(IRemoteOperator), url);
                if (op == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + url);

                byte[] code;
                RemoteAsyncSendOpDelegate RemoteSendDel = new RemoteAsyncSendOpDelegate(op.SendOperator);
                IAsyncResult RemSendAr;

                if (type.ToLower().Equals(SysConfig.CUSTOM))
                {
                    if (op_specs.Count != 3 || op_specs[0].Equals("") || op_specs[1].Equals("") || op_specs[2].Equals(""))
                        throw new WrongOpSpecsException("Custom Operator Specification needs 3 arguments.");
                    code = File.ReadAllBytes(op_specs[0]);
                    RemSendAr = RemoteSendDel.BeginInvoke(code, op_specs[1], op_specs[2], null, null, null);
                }
                else
                {
                    string className = "";
                    code = File.ReadAllBytes(LIB_OPERATORS_PATH);
                    if (type.ToLower().Equals(SysConfig.UNIQUE))
                        className = "UniqueOperator";
                    else if (type.ToLower().Equals(SysConfig.COUNT))
                        className = "CountOperator";
                    else if (type.ToLower().Equals(SysConfig.FILTER))
                        className = "FilterOperator";
                    else if (type.ToLower().Equals(SysConfig.DUP))
                        className = "DupOperator";

                    RemSendAr = RemoteSendDel.BeginInvoke(code, className, DEFAULT_METHOD, op_specs, null, null);
                }
                replicas.Add(op);
            }
            operators.Add(Int32.Parse(id), replicas);
        }

        private IList<IList<string>> doInputOps(String[] line, int i)
        {
            i++; // i is "sources"

            List<string> sources = new List<string>();
            IList<IList<string>> sources_processed = new List<IList<string>>();
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
                    sources_processed.Add(operators_addresses[op]);
                }
                else
                {
                    List<string> source = new List<string>();
                    source.Add(s);
                    sources_processed.Add(source);

                }
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
            manualM = true;
            string command="";
            while (!command.ToLower().Equals("q"))
            {
                command = Console.ReadLine();
                if(!command.ToLower().Equals("q"))
                    doCommandLine(command, 0);
            }
        }

        public void enterCommand(string command)
        {
            manualM = true;
            doCommandLine(command.ToLower(), 0);
            manualM = false;
        }

        public void shutDownAll()
        {
            foreach (KeyValuePair<int, IList<IRemoteOperator>> entry in operators)
            {
                IList<IRemoteOperator> replicas = operators[entry.Key];
                foreach(IRemoteOperator op in replicas)
                {
                    RemoteAsyncNoArgsOpDelegate RemoteDel = new RemoteAsyncNoArgsOpDelegate(op.crash);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
                }
                
            }
        }

        #region "Interface Methods"
        public void registerLog(string address, IList<string> tuples)
        {
            foreach (string tuple in tuples) {
                string log = address + ",<" + tuple + ">";
                Console.WriteLine(log);
                logInfo(log);
            }
        }

        #endregion

    }
}
