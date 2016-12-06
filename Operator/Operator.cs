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

        public delegate void RemoteAsyncLogDelegate(string address,IList<string> tuples);
        public delegate void RemoteAsyncReqTuplesDelegate(string receiver_routing, int receiverTarget, IList<string> receiver_urls);
        public delegate void RemoteAsyncProcessTuplesDelegate(IList<IList<string>> tuples);
        public delegate void RemoteAsyncSetPrimaryDelegate(bool value);


        private TcpChannel channel;

        private bool finalOperator=false;
        private string pmurl;
        private string opName;
        private IList<IList<string>> sources;
        private String repFact;
        private String routing;
        private IList<String> urls;
        private int port;
        private byte[] opTypeCode;
        private string className=null;
        private string method;
        private IList<string> opSpecs;
        private string semantics;
        private bool fullLoggingLevel;

        private IList<IList<string>> notSentTuples = new List<IList<string>>();
        private IList<IList<string>> notProcessedTuples = new List<IList<string>>();
        private Dictionary<int, IList<IList<string>>> res = new Dictionary<int, IList<IList<string>>>();


        private IList<IRemoteOperator> receivers= new List<IRemoteOperator>();
        string receiver_routing;
        private IRemotePuppetMaster puppet = null;

        private bool frozen;
        private bool primary;
        private int repId;
        private int random;
        private int receiver_target;

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
            Console.WriteLine("Registering Operator at "+ urls[0] + " ...");
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this,opName, typeof(IRemoteOperator));

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

        private bool processTuples(IList<IList<string>> tuples)
        {

            if (!canProcessTuples())
            {
                foreach (IList<string> tuple in tuples) {
                    notProcessedTuples.Add(tuple);
                }
               
                return false;
            }
            Assembly assembly = Assembly.Load(this.opTypeCode);
            Console.WriteLine("aa"+assembly);
            foreach (Type type in assembly.GetTypes())
            {
                Console.WriteLine("loloooooooooooooooooo");

                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + this.className))
                    {
                        object ClassObj = Activator.CreateInstance(type);
                        object[] args;
                        if (className.Equals("UniqueOperator") || className.Equals("FilterOperator") || className.Equals("CountOperator") || className.Equals("DupOperator"))
                            args = new object[] { tuples,this.opSpecs };
                        else
                            args = new object[] { tuples };
                        object resultObject = type.InvokeMember(this.method, BindingFlags.Default | BindingFlags.InvokeMethod,null, ClassObj, args);
                        IList<IList<string>> result = (IList<IList<string>>)resultObject;

                        foreach (IList<string> tuple in result)
                        {
                            string res = "";
                            foreach (string s in tuple)
                                res += s + ",";
                            Console.WriteLine("DEBUG:" + res);
                        }
                            

                        if (fullLoggingLevel)
                        {
                            RemoteAsyncLogDelegate RemoteDel = new RemoteAsyncLogDelegate(puppet.registerLog);
                            //converting to puppet master format
                            IList<string> res = new List<string>();
                            foreach (IList<string> l in result)
                            {
                                String temp = "";
                                if (l.Count > 0)
                                    temp += l[0];
                                for(int i=1;i<l.Count;i++)
                                    temp += ","+l[i];
                                res.Add(temp);
                            }
                            IAsyncResult RemAr = RemoteDel.BeginInvoke(urls[0], res, null, null);
                        }

                        if (receivers.Count==0 )
                        {
                            foreach (IList<string> tuple in result)
                                notSentTuples.Add(tuple);
                            if (finalOperator && notSentTuples.Count>0)
                            {
                                outputToFile(notSentTuples);
                                notSentTuples = new List<IList<string>>();
                            }
                        }
                        else
                        {
                            if (receiver_routing.Equals(SysConfig.PRIMARY))
                            {
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[0].doProcessTuples);
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(result, null, null);
                                //receiver.doProcessTuples(result);
                            }
                            else if (receiver_routing.Equals(SysConfig.RANDOM)) //Random Routing
                            {
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[receiver_target].doProcessTuples);
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(result, null, null);
                            }
                            else if (receiver_routing.StartsWith(SysConfig.HASHING)) //Hashing Routing
                            {
                                Dictionary<int, IList<IList<string>>> new_res = new Dictionary<int, IList<IList<string>>>();
                                string[] aux = this.receiver_routing.Split('(');
                                int field = Int32.Parse(aux[1].First() + "");
                                foreach (IList<string> tuple in result)
                                {
                                    int replica = Math.Abs(tuple[field - 1].GetHashCode()) % Int32.Parse(this.repFact);
                                    IList<IList<string>> set;
                                    if (!(new_res.ContainsKey(replica)))
                                    {
                                        set = new List<IList<string>>();
                                        set.Add(tuple);
                                        new_res.Add(replica, set);
                                    }
                                    else
                                    {
                                        set = res[replica];
                                        set.Add(tuple);
                                        new_res[replica] = set;
                                    }
                                }

                                this.res = new_res;
                                foreach (int rep in res.Keys)
                                {
                                    RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(receivers[rep].doProcessTuples);
                                    IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(res[rep], null, null);
                                }
                            }
                        }

                        notProcessedTuples = new List<IList<string>>();
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
                            processTuples(tuples);
                        }
                        else if (routing.StartsWith(SysConfig.HASHING)) //HASHING
                        {
                            string[] aux = this.routing.Split('(');
                            int field = Int32.Parse(aux[1].First()+"");
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
                            processTuples(tuples);
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
            bool aceptingSpaces=false;
            string result = "";
            foreach  (char c in s)
            {
                if (c == '\"')
                {
                    s += c;
                    aceptingSpaces = !aceptingSpaces;
                }
                if ((c == ' ' && aceptingSpaces) || c!=' ')
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
            Console.WriteLine("   Final Operator: " + finalOperator);
            Console.Write("   Active Replicas: " );
            foreach (string url in urls)
                Console.Write(url+"  ");
            Console.WriteLine("");
            Console.WriteLine("   Not processed Tuples: ");
            foreach (IList<string> tuple in notProcessedTuples)
            {
                string res = "";
                if (tuple.Count > 0)
                    res += tuple[0];
                for (int i = 1; i < tuple.Count; i++)
                    res += "," + tuple[i];
                Console.WriteLine("      " + res);
            }
            Console.WriteLine("");
            Console.WriteLine("End of status.");
            Console.WriteLine("");
        }

        public void crash()
        {
            // for console applications
            int i = 1;
            for (string url = urls[i]; i < urls.Count; i++, url = urls[i])
            {
                channel = new TcpChannel();
                IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), url);
                if (op == null)
                {
                    continue;
                }
                RemoteAsyncSetPrimaryDelegate RemoteDel = new RemoteAsyncSetPrimaryDelegate(op.setPrimary);
                IAsyncResult RemAr = RemoteDel.BeginInvoke(true, null, null);
                break;
            }
                System.Environment.Exit(0);
        }

        public void freeze()
        {
            frozen = true;
        }

        public void unfreeze()
        {
            frozen = false;
            if (notProcessedTuples.Count>0)
                processTuples(notProcessedTuples);
        }
        #endregion

        public void requestTuples(string receiverRouting, int receiverTarget, IList<string> receiverUrls)
        {
            this.receiver_routing = receiverRouting;
            this.receiver_target = receiverTarget;

            //Dictionary for hashing routing
            if (receiver_routing.StartsWith(SysConfig.HASHING) && this.res.Count==0 && notSentTuples.Count!=0)
            {
                string[] aux = this.receiver_routing.Split('(');
                int field = Int32.Parse(aux[1].First()+"");
                foreach (List<string> tuple in notSentTuples)
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
           
            foreach (string url in receiverUrls)
            {
                channel = new TcpChannel();
                IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), url);
                if (op == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + url);
                receivers.Add(op);
                if (notSentTuples.Count > 0)
                {
                    if ((this.receiver_routing.Equals(SysConfig.PRIMARY) && url.Equals(receiverUrls[0])))
                    {
                        RemoteAsyncProcessTuplesDelegate remoteDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                        IAsyncResult remoteResult = remoteDel.BeginInvoke(notSentTuples, null, null);
                        //receiver.doProcessTuples(notSentTuples);
                    }
                    else if (url.Equals(receiverUrls[this.receiver_target]) && this.receiver_routing.Equals(SysConfig.RANDOM)) //RANDOM ROUTING
                    {
                        RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                        IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(notSentTuples, null, null);
                    }
                    else if (this.receiver_routing.StartsWith(SysConfig.HASHING)) //HASHING ROUTING
                    {
                        foreach (int rep in res.Keys)
                        {
                            if (url.Equals(receiverUrls[rep]))
                            {
                                Console.WriteLine("enviei ");
                                Console.WriteLine(rep+ " ");
                                RemoteAsyncProcessTuplesDelegate remoteProcTupleDel = new RemoteAsyncProcessTuplesDelegate(op.doProcessTuples);
                                IAsyncResult remoteResult = remoteProcTupleDel.BeginInvoke(res[rep], null, null);
                            }
                        }
                    }
                }
            }

        }

        public void doProcessTuples(IList<IList<string>> tuples)
        {
            processTuples(tuples);
        }

        public void makeAsOutputOperator()
        {
            finalOperator = true;
            outputToFile(notSentTuples);
            notSentTuples = new List<IList<string>>();
        }


}
}
