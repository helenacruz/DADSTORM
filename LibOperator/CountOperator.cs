using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class CountOperator : IOperator
    {
        public IList<string> CustomOperation(IList<string> candidatTuples, IList<string> opSpecs)
        {
            IList<string> result = new List<string>();
            int count = 0;
            if (opSpecs!=null)
                throw new WrongOpSpecsException("Count Operator Specification has not arguments.");
            foreach (string candidat_tuple in candidatTuples)
                count++;

            result.Add(""+count);
            return result;
        }

    }
}
