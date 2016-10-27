using Shared_Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreation
{

    class Program
    {
        static void Main(string[] args)
        {
            ProcessCreation pc = new ProcessCreation();
            pc.registerPCS();
        }
    }

    class ProcessCreation : MarshalByRefObject, IRemoteProcessCreation
    {
        private const String UNIQUE_OPERATOR_PROCESS = @"..\uniqueOperator\uniqueOperator.exe";

        public ProcessCreation()
        {

        }

        public void registerPCS()
        {
            TcpChannel channel = new TcpChannel(SysConfig.PCS_PORT);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, SysConfig.PCS_NAME, typeof(IRemotePuppetMaster));
        }

        #region "Interface Methods"
        public void startOP(string type, string id, List<string> sources, String rep_fact, String routing, List<String> urls, int port, string op_specs)
        {
            Process process = new Process();
            if (type.Equals(SysConfig.UNIQUE)){
                process.StartInfo.FileName = UNIQUE_OPERATOR_PROCESS;

                string args = type + " " + id + " ";
                foreach (string s in sources)
                {
                    if (!sources[sources.Count - 1].Equals(s))
                        args += s + ",";
                    else
                        args += s + " ";
                }
                args += rep_fact + " " + routing + " ";
                foreach (string s in urls)
                {
                    if (!urls[urls.Count - 1].Equals(s))
                        args += s + ",";
                    else
                        args += s +" ";
                }
                int aux;
                if (Int32.TryParse(op_specs,out aux))
                     args += port + " " + aux;
                else
                    throw new OperatorSpecsException("Invalid operator spec, it uses an int as parameter");
                process.StartInfo.Arguments = args;
            }

            process.Start();
        }
        #endregion
    }
}
