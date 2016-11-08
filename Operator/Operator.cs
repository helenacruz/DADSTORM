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

namespace Operator
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Starting Operator ...");
            Operator op;
            if (args.Length==7)
                op = new Operator(args[0],args[1], getList(args[2]), args[3], args[4], getList(args[5]), Int32.Parse(args[6]));
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

    }

    class Operator : MarshalByRefObject, IRemoteOperator
    {
        private TcpChannel channel;

        private string pmurl;
        private string opName;
        private IList<string> sources;
        private String repFact;
        private String routing;
        private IList<String> urls;
        private int port;
        private byte[] opTypeCode;
        private string className=null;
        private string method;
        private IList<string> opSpecs;

        private IList<string> notSentTuples = new List<string>();

        private IList<string> receiverUrls=null;
        private IRemoteOperator receiver=null;

        private bool frozen;

        public Operator(string pmurl, string opName, List<string> sources, String repFact, String routing, List<String> urls, int port) {
            this.pmurl = pmurl;
            this.opName = opName;
            this.sources = sources;
            this.repFact = repFact;
            this.routing = routing;
            this.urls = urls;
            this.port = port;
            this.frozen = false;
        }

        public void registerOP()
        {
            Console.WriteLine("Registering Operator at "+ urls[0]);
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this,opName, typeof(IRemoteOperator));
            
        }

        private void sendRequestToSources(List<string> opsAsSources)
        {
            foreach (string source in opsAsSources)
            {
                channel = new TcpChannel();
                IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), source);
                if (op == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + source);
                op.requestTuples(urls);
            }
        }

        private bool canProcessTuples()
        {
            return frozen == false;
        }

        private bool processTuples(IList<string> tuples)
        {
            if (!canProcessTuples())
            {
                foreach (string tuple in tuples) {
                    notSentTuples.Add(tuple);
                }
               
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
                        if (className.Equals("UniqueOperator") || className.Equals("FilterOperator") || className.Equals("CountOperator") || className.Equals("DupOperator"))
                            args = new object[] { tuples,this.opSpecs };
                        else
                            args = new object[] { tuples };
                        object resultObject = type.InvokeMember(this.method, BindingFlags.Default | BindingFlags.InvokeMethod,null, ClassObj, args);
                        IList<string> result = (IList<string>)resultObject;
                        foreach (string tuple in result) 
                            Console.WriteLine(tuple);
                        if (receiver == null)
                        {
                            foreach (string tuple in result)
                                notSentTuples.Add(tuple);
                        }
                        else
                            receiver.doProcessTuples(result);
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));                
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

                if (!sources[0].Contains("tcp://"))
                {
                    string line;
                    StreamReader file;
                    file = new StreamReader("../../../Input/"+sources[0]);
                    List<string> tuples = new List<string>();
                    while ((line = file.ReadLine()) != null)
                    {
                        if(!line.StartsWith("%"))
                            tuples.Add(removeWhiteSpaces(line));
                    }
                    processTuples(tuples);
                }
                else
                {
                    ops_as_sources.Add(sources[0]);
                    sendRequestToSources(ops_as_sources);
                    Console.WriteLine("Waiting tuples from an operator");
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

        public void requestTuples(IList<string> urls)
        {
            receiverUrls = urls;
            channel = new TcpChannel();
            IRemoteOperator op = (IRemoteOperator)Activator.GetObject(typeof(Operator), receiverUrls[0]);
            if (op == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + receiverUrls[0]);
            receiver=op;
            receiver.doProcessTuples(notSentTuples);
        }

        public void doProcessTuples(IList<string> tuples)
        {
            processTuples(tuples);
        }

        public void interval(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void status()
        {
            // TODO 
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
        }

        public void wait(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }
        #endregion
    }
}
