using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{

    public interface IRemoteProcessCreation
    {
        void startOP(string type, string id, List<string> sources, String rep_fact, String routing, List<String> urls, int port, int field_number);
    }

    public interface IRemotePuppetMaster
    {
        void registerLog();
    }

    public interface IRemoteOperator
    {
        void requestTuples(List<string> urls);
        void processTuples(List<List<string>> tuples);
    }

}
