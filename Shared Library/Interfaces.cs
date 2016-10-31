using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{

    public interface IRemoteProcessCreation
    {
        void createOP(string pmurl, string id, List<string> sources, String rep_fact, String routing, List<String> urls, int port, string type, List<string> op_specs);
    }

    public interface IRemotePuppetMaster
    {
        void registerLog();
    }

    public interface IRemoteOperator
    {
        void startOperator();
        void requestTuples(List<string> urls);
        void processTuples(List<List<string>> tuples);
    }

}
