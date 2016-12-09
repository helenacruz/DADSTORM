using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using Shared_Library;
using System.IO;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Timers;

namespace Operator
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Starting Operator ...");
            Operator op;
            if (args.Length == 12)
                op = new Operator(args[0], args[1], getListofLists(args[2]), args[3], args[4], getList(args[5]), Int32.Parse(args[6]), args[7], args[8], args[9], args[10], args[11]);
            else
                throw new WrongOpSpecsException("The Operator must start with 7 arguments");

            op.registerOP();
            Console.WriteLine("Operator " + args[1] + " was started.");
            Console.ReadLine();

        }

        public static List<string> getList(string line)
        {
            return line.Split(',').ToList();
        }

        public static IList<IList<string>> getListofLists(string line)
        {
            IList<IList<string>> res = new List<IList<string>>();
            List<string> aux = line.Split(';').ToList();
            foreach (string s in aux)
            {
                res.Add(s.Split(',').ToList());
            }
            return res;
        }

    }

    class Operator : MarshalByRefObject, IRemoteOperator
    {

        public delegate void RemoteAsyncLogDelegate(string address, IList<string> tuples);
        public delegate void RemoteAsyncReqTuplesDelegate(string receiver_routing, int receiverTarget, IList<string> receiver_urls);
        public delegate void RemoteAsyncProcessTuplesDelegate(String machine, string seq, IList<IList<string>> tuples);
        public delegate void RemoteAsyncAckTuplesDelegate(string seq);
        public delegate void RemoteAsyncSetPrimaryDelegate(bool value);
        public delegate void RemoteAsyncPingDelegate(string machine);
        public delegate void RemoteAsyncPongDelegate(string machine);



        private TcpChannel channel;

        private bool finalOperator = false;
        private string pmurl;
        private string opName;
        private IList<IList<string>> sources;
        private String repFact;
        private String routing;
        private IList<String> urls;
        private int port;
        private byte[] opTypeCode;
        private string className = null;
        private string method;
        private IList<string> opSpecs;
        private string semantics;
        private bool fullLoggingLevel;
        private int seq = 0;
        private Timer timer1;
        public const int pingsLimit = 2;
        public const int notAckedLimit = 8;

        public const int pingsTimeouts = 2500;

        private Dictionary<string, int> pings = new Dictionary<string, int>();
        private Dictionary<string, int> notAckedCounters = new Dictionary<string, int>();

        private IList<string> uniqueTuples = new List<string>();

        private Dictionary<string, IList<IList<string>>> notSentTuples = new Dictionary<string, IList<IList<string>>>();
        private Dictionary<string, IList<IList<string>>> notProcessedTuples = new Dictionary<string, IList<IList<string>>>();
        private Dictionary<int, IList<IList<string>>> res = new Dictionary<int, IList<IList<string>>>();

        private Dictionary<string, IList<IList<string>>> not_acked = new Dictionary<string, IList<IList<string>>>();
        private Dictionary<string, string> relationingSequences = new Dictionary<string, string>();
        //mySequenceNumber--Source sequence number


        private IList<IRemoteOperator> receivers = new List<IRemoteOperator>();
        private IList<string> receivers_urls = new List<string>();
        private IList<string> receivers_bad_urls = new List<string>();

        string receiver_routing;
        private IRemotePuppetMaster puppet = null;

        private bool frozen;
        private bool primary;
        private int repId;
        private int random;
        private int receiver_target;
        private bool requested = false;


        public Operator(string pmurl, string opName, IList<IList<string>> sources, String repFact, String routing, List<String> urls, int port, string semantics, string loggingLevel, string primary, string repId, string random)
        {
            this.pmurl = pmurl;
            this.opName = opName;
            this.sources = sources;
            this.repFact = repFact;
            this.routing = routing;
            this.urls = urls;
            this.port = port;
            this.frozen = false;
            this.semantics = semantics;
            this.repId = Int32.Parse(repId);
            this.random = Int32.Parse(random);

            this.fullLoggingLevel = true;
            if (loggingLevel.Equals(SysConfig.LIGHT))
                this.fullLoggingLevel = false;

            this.primary = true;
            if (primary.ToLower().Equals("false"))
                this.primary = false;

            cleanFile();

        }

        public void registerOP()
        {
            Console.WriteLine("Registering Operator at " + urls[0] + " ...");
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, opName, typeof(IRemoteOperator));

            channel = new TcpChannel();
            puppet = (IRemotePuppetMaster)Activator.GetObject(typeof(Operator), pmurl);
            if (puppet == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Puppet Master from " + pmurl);

        }

        private void sendRequestToSources(IList<string> opsAsSources)
        {
            //so o primario faz request às sources
            if (primary && this.routing.Equals(SysConfig.PRIMARY) || repId == random && routing.Equals(SysConfig.RANDOM) || primary && routing.StartsWith(SysConfig.HASHING))
            {
                foreach (string source in opsAsSources)
                {
                    channel = new TcpChannel();
                    IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), source);
                    if (op == null)
                        throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + source);
                    RemoteAsyncReqTuplesDelegate remoteDel = new RemoteAsyncReqTuplesDelegate(op.requestTuples);
                    IAsyncResult remoteResult = remoteDel.BeginInvoke(routing, random, urls, null, null);
                    //op.requestTuples(urls);
                }
            }
        }

        private bool canProcessTuples()
        {
            return frozen == false;
        }

        private bool processTuples(string machine, string machine_seq, IList<IList<string>> tuples)
        {

            if (!canProcessTuples())
            {
                notProcessedTuples.Add(machine + ";" + machine_seq, tuples);
                return false;
            }

            Assembly assembly = Assembly.Load(this.opTypeCode);

            foreach (Type type in assembly.GetTypes())
            {


                if (type.IsClass == true)
                {

                    if (type.FullName.EndsWith("." + this.className))
                    {
                        object ClassObj = Activator.CreateInstance(type);
                        object[] args;
                        object resultObject;
                        IList<IList<string>> result = new List<IList<string>>();

                        string resu = "";
                        foreach (IList<string> tuple in tuples)
                        {
                            foreach (string s in tuple)
                                resu += s + ",";
                            resu += "\n";
                        }
                        //Console.WriteLine("DEBUG Before Processing:" + resu);

                        if (className.Equals("UniqueOperator") || className.Equals("FilterOperator") || className.Equals("CountOperator") || className.Equals("DupOperator"))
                        {
                            
                            args = new object[] { tuples, this.opSpecs };
                            resultObject = type.InvokeMember(this.method, BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, args);
                            result = (IList<IList<string>>)resultObject;

                            if (className.Equals("UniqueOperator"))
                            {
                                IList<IList<string>> aux2 = new List<IList<string>>();
                                foreach (IList<string> tup in result)
                                {
                                   
                                    foreach (string str in tup)
                                    {
                                        IList<string> aux = new List<string>();
                                        bool unique = true;
                                        foreach (string str_unique in uniqueTuples)
                                        {
                                            if (str_unique.Equals(str))
                                            {
                                                unique = false;
                                                break;
                                            }
                                        }
                                        if (unique)
                                        {
                                            uniqueTuples.Add(str);
                                            aux.Add(str);
                                            aux2.Add(aux);
                                        }
                                    }
                                }
                                result = aux2;
                            }
                        }
                        else
                        {

                            foreach (IList<string> tuple in tuples)
                            {
                                args = new object[] { tuple };
                                resultObject = type.InvokeMember(this.method, BindingFlags.Default | BindingFlags.InvokeMethod, null, ClassObj, args);
                                IList<IList<string>> temp = (IList<IList<string>>)resultObject;
                                foreach (List<string> t in temp)
                                    result.Add(t);
                            }


                        }

                        resu = "";
                        foreach (IList<string> tuple in result)
                        {
                            foreach (string s in tuple)
                                resu += s + ",";
                            resu += "\n";
                        }
                        //Console.WriteLine("DEBUG After Processing:" + resu);

                        if (fullLoggingLevel && result.Count>0)
                        {
                            RemoteAsyncLogDelegate RemoteDel = new RemoteAsyncLogDelegate(puppet.registerLog);
                            //converting to puppet master format
                            IList<string> res = new List<string>();
                            foreach (IList<string> l in result)
                            {
                                String temp = "";
                                if (l.Count > 0)
                                    temp += l[0];
                                for (int i = 1; i < l.Count; i++)
                                    temp += "," + l[i];
                                res.Add(temp);
                            }
                            IAsyncResult RemAr = RemoteDel.BeginInvoke(urls[0], res, null, null);
                        }

                        if (receivers.Count == 0 && result.Count>0)
                        {
                            notSentTuples.Add(machine + ";" + machine_seq, result);
                            if (finalOperator && notSentTuples.Count > 0)
                            {
                                sendAckToPrevious(machine, machine_seq);
                                //outputToFile(notSentTuples);
                                notSentTuples = new Dictionary<string, IList<IList<string>>>();
                            }
                        }
                        else
                        {
                            if (receiver_routing.Equals(SysConfig.PRIMARY) && result.Count>0)
                            {
                                relationingSequences.Add("" + seq, "" + machine_seq);
                                not_acked.Add(machine + ";" + seq + ";" + receivers_urls[0], result);
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[0].doProcessTuples);
                                seq += 1;
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(this.urls[0], "" + seq, result, null, null);
                                //receiver.doProcessTuples(result);
                            }
                            else if (receiver_routing.Equals(SysConfig.RANDOM) && result.Count > 0) //Random Routing
                            {
                                Random r = new Random();
                                receiver_target = r.Next() % receivers.Count();

                                relationingSequences.Add("" + seq, "" + machine_seq);
                                not_acked.Add(machine + ";" + seq + ";" + receivers_urls[receiver_target], result);
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[receiver_target].doProcessTuples);
                                seq += 1;
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(this.urls[0], "" + seq, result, null, null);
                            }
                            else if (receiver_routing.StartsWith(SysConfig.HASHING) && result.Count > 0) //Hashing Routing
                            {
                                string[] aux = this.receiver_routing.Split('(');
                                int field = Int32.Parse(aux[1].First() + "");
                                res = new Dictionary<int, IList<IList<string>>>();

                                foreach (IList<string> tuple in result)
                                {
                                    int replica = Math.Abs(tuple[field - 1].GetHashCode()) % Int32.Parse(this.repFact);

                                    if (!res.ContainsKey(replica))
                                    {
                                        IList<IList<string>> temp = new List<IList<string>>();
                                        temp.Add(tuple);
                                        res.Add(replica,temp);
                                    }
                                    else
                                    {
                                        res[replica].Add(tuple);
                                    }
                                }

                                foreach (int rep in res.Keys)
                                {
                                    relationingSequences.Add("" + seq, "" + machine_seq);
                                    not_acked.Add(machine + ";" + seq + ";" + receivers_urls[rep], res[rep]);
                                    RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[rep].doProcessTuples);
                                    seq += 1;
                                    IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(this.urls[0], "" + seq, res[rep], null, null);
                                }

                            }
                        }

                        notProcessedTuples = new Dictionary<string, IList<IList<string>>>();
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
        }

        public void outputToFile(IList<IList<string>> candidatTuples)
        {
            string outputFile = @"../../../Output.txt";

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(outputFile, true))
            {
                foreach (IList<string> tuple in candidatTuples)
                {
                    String line = "";
                    foreach (string s in tuple)
                        line += tuple;
                    file.WriteLine(line);
                }
            }
        }

        public void cleanFile()
        {
            System.IO.File.WriteAllText(@"../../../Output.txt", string.Empty);
        }

        #region "Interface Methods"


        public void SendOperator(byte[] code, string className, string method, IList<string> opSpecs)
        {
            this.opTypeCode = code;
            this.className = className;
            this.method = method;
            this.opSpecs = opSpecs;
        }


        public void startOperator()
        {
            if(semantics.Equals(SysConfig.AT_LEAST_ONCE))
                InitTimer();
            if (opTypeCode != null)
            {
                Console.WriteLine("Operator started processing...");

                List<string> ops_as_sources = new List<string>();

                foreach (List<string> source in sources)
                {
                    if (!source[0].Contains("tcp://"))
                    {
                        string line;
                        StreamReader file;
                        file = new StreamReader("../../../Input/" + source[0]);
                        IList<IList<string>> tuples = new List<IList<string>>();

                        if ((routing.Equals(SysConfig.PRIMARY) && primary) || (routing.Equals(SysConfig.RANDOM) && repId == random))
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                if (!line.StartsWith("%"))
                                {
                                    string[] splited = removeWhiteSpaces(line).Split(',');
                                    IList<string> l = new List<string>();
                                    foreach (string s in splited)
                                        l.Add(s);
                                    tuples.Add(l);

                                }
                            }
                            processTuples(this.urls[0], "-1", tuples);
                        }
                        else if (routing.StartsWith(SysConfig.HASHING)) //HASHING
                        {
                            string[] aux = this.routing.Split('(');
                            int field = Int32.Parse(aux[1].First() + "");
                            while ((line = file.ReadLine()) != null)
                            {
                                string[] split = line.Split(',');
                                int replica = Math.Abs(split[field - 1].GetHashCode()) % Int32.Parse(this.repFact);
                                if (!line.StartsWith("%") && replica == this.repId)
                                {
                                    string[] splited = removeWhiteSpaces(line).Split(',');
                                    IList<string> l = new List<string>();
                                    foreach (string s in splited)
                                        l.Add(s);
                                    tuples.Add(l);
                                }
                            }
                            processTuples(this.urls[0], "-1", tuples);
                        }
                    }
                    else
                    {
                        sendRequestToSources(source);
                        Console.WriteLine("Waiting tuples from an operator...");

                    }
                }

            }
            else
                throw new OpByteCodesNotReceivedException("The operator " + opName + " at " + urls[0] + "not received operator byte codes");
        }

        private string removeWhiteSpaces(string s)
        {
            bool aceptingSpaces = false;
            string result = "";
            foreach (char c in s)
            {
                if (c == '\"')
                {
                    s += c;
                    aceptingSpaces = !aceptingSpaces;
                }
                if ((c == ' ' && aceptingSpaces) || c != ' ')
                    result += c;
            }
            return result;
        }


        public void interval(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void setPrimary(bool value)
        {
            primary = value;
        }

        public void status()
        {
            Console.WriteLine("");
            Console.WriteLine("Status of " + opName + " at " + urls[0] + ":");
            Console.WriteLine("   Class Name: " + className);
            Console.WriteLine("   Method: " + method);
            Console.WriteLine("   Primary: " + primary);
            Console.WriteLine("   Replica number: " + repId);
            Console.WriteLine("   Target replica: " + random);

            if (frozen)
                Console.WriteLine("   State: Frozen.");
            else
                Console.WriteLine("   State: Not Frozen.");
            Console.WriteLine("   Replication factory: " + repFact);
            Console.WriteLine("   My Routing: " + routing);
            Console.WriteLine("   Receiver Routing: " + receiver_routing);
            Console.WriteLine("   Semantics: " + semantics);
            Console.WriteLine("   Final Operator: " + finalOperator);
            Console.Write("   Active Replicas: ");
            foreach (string url in urls)
                Console.Write(url + "  ");
            Console.WriteLine("");
            Console.Write("   Receiver Urls: ");
            foreach (string url in this.receivers_urls)
                Console.Write(url + "  ");
            Console.WriteLine("");
            Console.WriteLine("   Not processed Tuples: ");
            foreach (KeyValuePair<string, IList<IList<string>>> entry in notProcessedTuples)
            {
                foreach (IList<string> tuple in entry.Value)
                {
                    string res = "";
                    if (tuple.Count > 0)
                        res += tuple[0];
                    for (int i = 1; i < tuple.Count; i++)
                        res += "," + tuple[i];
                    Console.WriteLine("      " + res);
                }
            }
            Console.WriteLine("");
            Console.WriteLine("   Not Sent Tuples: ");
            foreach (KeyValuePair<string, IList<IList<string>>> entry in notSentTuples )
            {
                foreach (IList<string> tuple in entry.Value)
                {
                    string res = "";
                    if (tuple.Count > 0)
                        res += tuple[0];
                    for (int i = 1; i < tuple.Count; i++)
                        res += "," + tuple[i];
                    Console.WriteLine("      " + res);
                }
            }
            Console.WriteLine("");
            Console.WriteLine("   Not Acked Tuples: ");
            foreach (KeyValuePair<string, IList<IList<string>>> entry in not_acked )
            {
                foreach (IList<string> tuple in entry.Value)
                {
                    string res = "";
                    if (tuple.Count > 0)
                        res += tuple[0];
                    for (int i = 1; i < tuple.Count; i++)
                        res += "," + tuple[i];
                    Console.WriteLine("      " + res);
                }
            }
            Console.WriteLine("");
            Console.WriteLine("End of status.");
            Console.WriteLine("");
        }

        public void crash()
        {
            // for console applications            
            System.Environment.Exit(0);
        }

        public void freeze()
        {
            frozen = true;
        }

        public void unfreeze()
        {
            frozen = false;
            if (notProcessedTuples.Count > 0)
            {
                foreach (KeyValuePair<string, IList<IList<string>>> entry in notProcessedTuples)
                {
                    string machine = entry.Key.Split(';')[0];
                    string sequence = entry.Key.Split(';')[1];
                    processTuples(machine, sequence, entry.Value);

                }
            }
        }
        #endregion

        public void requestTuples(string receiverRouting, int receiverTarget, IList<string> receiverUrls)
        {
            if (!requested) {
                requested = true;
                this.receiver_routing = receiverRouting;
                this.receiver_target = receiverTarget;
                //Dictionary for hashing routing
                if (receiver_routing.StartsWith(SysConfig.HASHING) && this.res.Count == 0 && notSentTuples.Count != 0)
                {
                    string[] aux = this.receiver_routing.Split('(');
                    int field = Int32.Parse(aux[1].First() + "");

                    foreach (KeyValuePair<string, IList<IList<string>>> entry in notProcessedTuples)
                    {
                        foreach (List<string> tuple in entry.Value)
                        {
                            int replica = Math.Abs(tuple[field - 1].GetHashCode()) % receiverUrls.Count;
                            IList<IList<string>> set;
                            if (!(res.ContainsKey(replica)))
                            {
                                set = new List<IList<string>>();
                                set.Add(tuple);
                                res.Add(replica, set);
                            }
                            else
                            {
                                set = res[replica];
                                set.Add(tuple);
                                res[replica] = set;
                            }
                        }
                    }
                }

                foreach (string url in receiverUrls)
                {
                    channel = new TcpChannel();
                    IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), url);
                    if (op == null)
                        throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + url);
                    receivers.Add(op);
                    this.receivers_urls.Add(url);
                    if (notSentTuples.Count > 0)
                    {
                        if ((this.receiver_routing.Equals(SysConfig.PRIMARY) && url.Equals(receiverUrls[0])))
                        {
                            //ERROR can be here, there was notProcessedTuples
                            foreach (KeyValuePair<string, IList<IList<string>>> entry in notSentTuples)
                            {

                                string machine = entry.Key.Split(';')[0];
                                string sequence = entry.Key.Split(';')[1];
                                relationingSequences.Add("" + seq, "" + sequence);
                                not_acked.Add(machine+";"+seq + ";" + receivers_urls[0], entry.Value);
                                RemoteAsyncProcessTuplesDelegate remoteDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                                seq += 1;
                                IAsyncResult remoteResult = remoteDel.BeginInvoke(urls[0], ""+seq, entry.Value, null, null);
                                //receiver.doProcessTuples(notSentTuples);
                            }
                        }
                        else if (url.Equals(receiverUrls[0]) && this.receiver_routing.Equals(SysConfig.RANDOM)) //RANDOM ROUTING
                        {
                            foreach (KeyValuePair<string, IList<IList<string>>> entry in notSentTuples)
                            {

                                string machine = entry.Key.Split(';')[0];
                                string sequence = entry.Key.Split(';')[1];
                                relationingSequences.Add("" + seq, "" + sequence);
                                not_acked.Add(machine + ";" + seq + ";" + receivers_urls[0], entry.Value);
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                                seq += 1;
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(urls[0], ""+seq, entry.Value, null, null);
                            }
                        }
                        else if (this.receiver_routing.StartsWith(SysConfig.HASHING)) //HASHING ROUTING
                        {
                            foreach (int rep in res.Keys)
                            {
                                if (url.Equals(receiverUrls[rep]))
                                {
                                    relationingSequences.Add("" + seq, "" + seq);
                                    not_acked.Add(this.urls[0] + ";" + seq + ";" + url, res[rep]);
                                    RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                                    seq += 1;
                                    IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(this.urls[0], "" + seq, res[rep], null, null);
                                }
                            }
                        }
                        notSentTuples = new Dictionary<string, IList<IList<string>>>();
                    }
                }
            }
            else
            {
                foreach (string url in receivers_bad_urls)
                    sendDeadReplica(url);
                receivers_bad_urls = new List<string>();
            }

        }

        public void doProcessTuples(String machine, string machine_seq, IList<IList<string>> tuples)
        {
            foreach (KeyValuePair<string, IList<IList<string>>> entry in not_acked)
            {
                string[] splited = entry.Key.Split(';');
                string mach = splited[0];
                string actualSeq = splited[1];

                if (machine.Equals(mach) && machine_seq.Equals(machine_seq))
                {
                    return;
                }
                    
            }
             processTuples(machine, machine_seq, tuples);
        }

        public void doAckTuples(string sequence)
        {
            foreach (KeyValuePair<string, IList<IList<string>>> entry in not_acked)
            {
                string[] splited = entry.Key.Split(';');
                string machine = splited[0];
                string actualSeq = splited[1];

                if (actualSeq.Equals(sequence))
                {
                    string previousMachineSeq = relationingSequences[actualSeq];
                    if (!previousMachineSeq.Equals("-1"))
                    {
                        sendAckToPrevious(machine, previousMachineSeq);
                    }
                   
                    not_acked.Remove(entry.Key);
                    relationingSequences.Remove(sequence);
                    return;
                }

            }

        }

        public void sendAckToPrevious(string url, string previousMachineSeq)
        {
            channel = new TcpChannel();
            IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), url);
            if (op == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + url);
            //RemoteAsyncAckTuplesDelegate remoteDel = new RemoteAsyncAckTuplesDelegate(op.doAckTuples);
            //IAsyncResult remoteResult = remoteDel.BeginInvoke(previousMachineSeq, null, null); 
            if (semantics.Equals(SysConfig.AT_LEAST_ONCE))
                op.doAckTuples(previousMachineSeq);
        }

        public void makeAsOutputOperator()
        {
            finalOperator = true;
            foreach (KeyValuePair<string, IList<IList<string>>> entry in notSentTuples)
            {
                string[] splited = entry.Key.Split(';');
                string machine = splited[0];
                string actualSeq = splited[1];
                string previousMachineSeq = relationingSequences[actualSeq];
                sendAckToPrevious(machine, previousMachineSeq);
            }
            //outputToFile(notSentTuples);
            notSentTuples = new Dictionary<string, IList<IList<string>>>();
        }

        public void ping(string machine)
        {
            channel = new TcpChannel();
            IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), machine);
            if (op == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote replica Operator from " + machine);

            RemoteAsyncPongDelegate remoteDeleg = new RemoteAsyncPongDelegate(op.pong);
            IAsyncResult remoteResulte = remoteDeleg.BeginInvoke(urls[0], null, null);
        }
        public void pong(string machine)
        {
            pings[machine] = 0;
        }

        public void sendDeadReplica(string replica)
        {
            receivers_bad_urls.Add(replica);

            for (int i=0; i<receivers_urls.Count;i++)
            {

                if (receivers_urls[i].Equals(replica))
                {
                    receivers_urls.RemoveAt(i);
                    receivers.RemoveAt(i);

                    List<string> toRemove = new List<string>();
                    foreach (KeyValuePair<string, IList<IList<string>>> entry in not_acked)
                    {
                        string[] splited = entry.Key.Split(';');
                        string machine = splited[0];
                        string actualSeq = splited[1];
                        string destinationMachine = splited[2];
                        if (destinationMachine.Equals(replica))
                        {
                            toRemove.Add(entry.Key);
                            doProcessTuples(machine, actualSeq, entry.Value);
                        }
                    }
                    foreach(string s in toRemove)
                        not_acked.Remove(s);

                }
            }
        }


        private void InitTimer()
        {
            timer1 = new Timer();
            timer1.Elapsed += new ElapsedEventHandler(pingFunction);
            timer1.Interval = pingsTimeouts; 
            timer1.Start();
        }

        private void pingFunction(object sender, EventArgs e)
        {
            //resends requests if a lot of time passes
            foreach (KeyValuePair<string, IList<IList<string>>> entry in not_acked)
            {
                if (!notAckedCounters.ContainsKey(entry.Key))
                {
                    notAckedCounters.Add(entry.Key, 0);
                }
                notAckedCounters[entry.Key] += 1;
                if (notAckedCounters[entry.Key] == notAckedLimit)
                {
                    notAckedCounters.Remove(entry.Key);
                    
                    string[] splited = entry.Key.Split(';');
                    string machine = splited[0];
                    string actualSeq = splited[1];
                    string destinationMachine = splited[2];
                        
                    doProcessTuples(machine, actualSeq, entry.Value);
                    not_acked.Remove(entry.Key);

                }
            }
            //end of resend

            foreach (string url in urls)
            {
                if (!pings.ContainsKey(url))
                    pings[url] = 0;

                if (!url.Equals(url[0]))
                {

                    if (pings[url] >= pingsLimit)
                    {
                        deadReplicaDetected(url);
                        return;
                    }

                    pings[url] +=1;
                    channel = new TcpChannel();
                    IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), url);
                    if (op == null)
                        throw new CannotAccessRemoteObjectException("Cannot get remote replica Operator from " + url);

                    RemoteAsyncPingDelegate remoteDeleg = new RemoteAsyncPingDelegate(op.ping);
                    IAsyncResult remoteResulte = remoteDeleg.BeginInvoke(urls[0], null, null);
                }
                
                          
               }
          }

        private void deadReplicaDetected(string url)
        {
            if (!primary)
            {
                electPrimaryOperator(url);
                //we will try to start this operator like from 0 and send request to sources
                startOperator();
            }
            foreach (IList<string> source in sources)
            {
                foreach (string sourc in source)
                {
                    if (sourc.Contains("tcp://"))
                    {
                        channel = new TcpChannel();
                        IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), sourc);
                        if (op == null)
                            throw new CannotAccessRemoteObjectException("Cannot get remote replica Operator from " + sourc);
                        op.sendDeadReplica(url);
                    }
                       
                }
            }
            urls.Remove(url);
        }

        private void electPrimaryOperator(string deadUrl)
        {
            string primaryy = urls[0];
            for(int i=1;i<urls.Count;i++)
            {
                if (!urls[i].Equals(deadUrl))
                {
                    Version a = new Version(primaryy);
                    Version b = new Version(urls[i]);
                    if (a > b)
                        primaryy = urls[i];
                }   
            }
            if (urls[0].Equals(primaryy))
                this.primary = true;
        }


    }
}
