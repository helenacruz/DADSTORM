﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class PuppetMaster : MarshalByRefObject, IRemotePuppetMaster
    {

    }
}
