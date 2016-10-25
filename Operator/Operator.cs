using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using Shared_Library;

namespace Operator
{

    abstract class Operator : MarshalByRefObject, IRemoteOperator
    {
        private string id;
        private List<string> sources;
        private String rep_fact;
        private String routing;
        private List<String> urls;
        private int port;
        List<List<string>> queue_tuples;
        List<List<string>> processed_tuples;
        List<List<string>> not_sent_tuples;

        private List<string> receiver_urls=null;
        private Operator receiver=null;

        public Operator(string id, List<string> sources, String rep_fact, String routing, List<String> urls, int port) {
            this.id = id;
            this.sources = sources;
            this.rep_fact = rep_fact;
            this.routing = routing;
            this.urls = urls;
            this.port = port;

            sendRequestToSources();
        }

        public List<string> Sources
        {
            get{
                return sources;
            }
            set{
                sources = value;
            }
        }

        public List<List<string>> Queue
        {
            get
            {
                return queue_tuples;
            }
            set
            {
                queue_tuples = value;
            }
        }

        public Operator Receiver
        {
            get
            {
                return receiver;
            }
            set
            {
                receiver = value;
            }
        }

        public void queueTuple(List<string> tuple)
        {
            queue_tuples.Add(tuple);
        }
        //splits by , and gets all strings
        public static List<string> getList(string line)
        {
            return line.Split(',').ToList();
        }

        private void registerOP()
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this,id, typeof(IRemoteOperator));
        }
        public abstract void startOP();
        public abstract void processQueue();

        private void sendRequestToSources()
        {
            foreach (string source in sources) {
                TcpChannel channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel,false);
                Operator op = (Operator)Activator.GetObject(typeof(Operator), source);
                if (op == null)
                    throw new CannotAccessRemoteObjectException("Cannot get remote Operator from " + source);
                op.requestTuples(urls);
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
            queue_tuples.AddRange(tuples);
            processQueue();
        }
        #endregion
    }
}
