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
        private int id;
        private List<string> sources;
        private String rep_fact;
        private String routing;
        private List<String> urls;
        private int port;

        public Operator(int id, List<string> sources, String rep_fact, String routing, List<String> urls, int port) {
            this.id = id;
            this.sources = sources;
            this.rep_fact = rep_fact;
            this.routing = routing;
            this.urls = urls;
            this.port = port;
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
            RemotingServices.Marshal(this, ""+id, typeof(IRemoteOperator));
        }

    }
}
