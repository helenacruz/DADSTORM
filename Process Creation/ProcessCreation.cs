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
            Console.WriteLine("Starting Process Creation Service...");
            ProcessCreation pc = new ProcessCreation();
            pc.registerPCS();
            Console.WriteLine("Process Creation Service is running...");
            Console.ReadLine();
        }
    }

    class ProcessCreation : MarshalByRefObject, IRemoteProcessCreation
    {
        private const String UNIQUE_OPERATOR_PROCESS = @"..\..\..\Operator\bin\Debug\Operator.exe";

        public ProcessCreation()
        {

        }

        public void registerPCS()
        {
            TcpChannel channel = new TcpChannel(SysConfig.PCS_PORT);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, SysConfig.PCS_NAME, typeof(IRemoteProcessCreation));
        }

        #region "Interface Methods"
        public void createOP(string primary, string opName,string port,SysConfig sysConfig,string pmurl, IList<IList<string>> sources, String rep_fact, String routing, IList<String> urls)
        {
            Console.WriteLine("Creating operator "+opName+" at "+urls[0]);
    
            Process process = new Process();

            process.StartInfo.FileName = UNIQUE_OPERATOR_PROCESS;
            int j = 0 ;

            string args = pmurl + " " + opName + " ";
            for (int i=0;i<sources.Count;i++)
            {
                for (int k=0; k<sources[i].Count;k++)
                {
                    if (sources[i].Count - 1 == k)
                        args += sources[i][k] + " ";
                    else
                        args += sources[i][k] + ",";
                }
                if (i!=sources.Count-1)
                    args += ";";
            }
            j = 0;
            args += rep_fact + " " + routing + " ";
            foreach (string s in urls)
            {
                if (urls.Count - 1==j)
                    args += s + " ";
                else
                    args += s +",";
                j += 1;
            }
            args += port+" "+sysConfig.Semantics+" "+sysConfig.LoggingLevel + " " + primary;

            //Console.WriteLine("Using the following parameters: "+args);
            process.StartInfo.Arguments = args;
            process.Start();
            Console.WriteLine("Operator " + opName + " was created.");
        }
        #endregion
    }
}
