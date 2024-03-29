﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{

    public interface IRemoteProcessCreation
    {
        void createOP(string primary, string opName, string port, SysConfig sysConfig, string pmurl, IList<IList<string>> sources, String rep_fact, String routing, IList<String> urls, int repId, int random);
    }

    public interface IRemotePuppetMaster
    {
        void registerLog(string address, IList<string> tuples);
    }

    public interface IRemoteOperator
    {
        void SendOperator(byte[] code, string className, string method, IList<string> op_specs);
        void startOperator();
        void interval(int milliseconds);
        void status();
        void crash();
        void freeze();
        void unfreeze();
        void requestTuples(string receiver_routing, int receiverTarget, IList<string> receiver_urls);
        void doProcessTuples(string machine, string seq, IList<IList<string>> tuples);
        void doAckTuples(string seq);
        void makeAsOutputOperator();
        void setPrimary(bool value);
        void ping(string machine);
        void pong(string machine);
        void sendDeadReplica(string machine);

    }

    public interface IOperator
    {
        IList<IList<string>> CustomOperation(IList<IList<string>> tuples, IList<string> op_specs);
    }

}
