using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{

    public interface IRemoteProcessCreation
    {
        void createOP(string pmurl, string id, IList<string> sources, String rep_fact, String routing, IList<String> urls, int port);
    }

    public interface IRemotePuppetMaster
    {
        void registerLog();
    }

    public interface IRemoteOperator
    {
        void SendOperator(byte[] code, string className, string method, IList<string> op_specs);
        void startOperator();
        void requestTuples(IList<string> urls);
        void doProcessTuples(IList<string> tuples);
    }

    public interface IOperator
    {
        IList<string> CustomOperation(IList<string> tuples, IList<string> op_specs);
    }

}
