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

namespace Operator
{
    class Program
    {
        static void Main(string[] args)
        {
            Operator op = new Operator(args[0],args[1], getList(args[3]), args[4], args[5], getList(args[6]), Int32.Parse(args[7]), args[8] ,getList(args[9]));
            op.registerOP();
            if(args[8] == SysConfig.UNIQUE)
            {
                op.startUniqueOp();
            }

        }

        public static List<string> getList(string line)
        {
            return line.Split(',').ToList();
        }

    }

    class Operator : MarshalByRefObject, IRemoteOperator
    {
        private string pmurl;
        private string id;
        private List<string> sources;
        private String rep_fact;
        private String routing;
        private List<String> urls;
        private int port;
        private string op_type;
        List<string> op_specs;

        List<List<string>> tuples_queue;
        List<List<string>> not_sent_tuples;

        private List<string> receiver_urls=null;
        private Operator receiver=null;

        public Operator(string pmurl, string id, List<string> sources, String rep_fact, String routing, List<String> urls, int port, string op_type, List<string> op_specs) {
            this.pmurl = pmurl;
            this.id = id;
            this.sources = sources;
            this.rep_fact = rep_fact;
            this.routing = routing;
            this.urls = urls;
            this.port = port;
            this.op_type = op_type;
            this.op_specs = op_specs;

            sendRequestToSources();
        }

        public void registerOP()
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this,id, typeof(IRemoteOperator));
        }

        private void sendRequestToSources()
        {
            foreach (string source in sources)
            {
                TcpChannel channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel, false);
                Operator op = (Operator)Activator.GetObject(typeof(Operator), source);
                if (op == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + source);
                op.requestTuples(urls);
            }
        }

        public void startUniqueOp()
        {
            if (!sources[0].Contains("tcp://"))
            {
                string line;
                StreamReader file;

                file = new StreamReader(sources[0]);

                while ((line = file.ReadLine()) != null)
                {
                    processTupleUniqueOp(line.Split(' ').ToList());
                }
            }
            else
                Console.WriteLine("Waiting tuples from an operator");
        }

        private void processTupleUniqueOp(List<string> tuple)
        {
            int field_number;
            if (!Int32.TryParse(op_specs[0], out field_number))
                throw new WrongOpSpecsException("Unique Operator Specification need to be an integer.");

            foreach (List<string> unique_tuple in tuples_queue)
                if (!tuple[field_number - 1].Equals(unique_tuple[field_number - 1]))
                {
                    tuples_queue.Add(tuple);

                    if (receiver != null)
                    {
                        List<List<string>> to_fix = new List<List<String>>();
                        to_fix.Add(tuple);
                        receiver.processTuples(to_fix);
                    }

                    string aux = "";
                    foreach (string s in tuple)
                        aux += s;
                    Console.WriteLine(tuple);
                }
        }

        #region "Interface Methods"
        public void requestTuples(List<string> urls)
        {
            receiver_urls = urls;
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            Operator op = (Operator)Activator.GetObject(typeof(Operator), receiver_urls[0]);
            if (op == null)
                throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + receiver_urls[0]);
            receiver=op;
        }
        public void processTuples(List<List<string>> tuples)
        {
            foreach (List<string>  t in tuples) {
                processTupleUniqueOp(t);
            }
        }
        #endregion
    }
}
