using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Library
{
   
    public interface IRemotePuppetMaster
    {
        void registerLog();
    }



    public interface IRemoteOperator
    {
        void start();
    }

}
