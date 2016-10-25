using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{

    class Program
    {
        static void Main(string[] args)
        {
            UniqueOperator op = new UniqueOperator(Int32.Parse(args[0]), Operator.getList(args[1]), args[2], args[3], Operator.getList(args[4]), Int32.Parse(args[5]), Int32.Parse(args[6]));
        }

    }

    class UniqueOperator : Operator
    {
        private int field_number;

        public UniqueOperator (int id, List<string> sources, string rep_fact, string routing, List<string> urls, int port, int field_numbe) : base (id, sources, rep_fact, routing, urls, port)
        {
            this.field_number = field_number;
        }

        #region "Interface Methods"
        public void start()
        {

        }

        #endregion

    }
}
