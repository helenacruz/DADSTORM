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
        public IList<IList<string>> CustomOperation(IList<IList<string>> candidatTuples, IList<string> opSpecs)
        {
            IList<IList<string>> result = new List<IList<string>>();
            int count = 0;
            if (opSpecs!=null)
                throw new WrongOpSpecsException("Count Operator Specification has not arguments.");
            foreach (IList<string> candidat_tuple in candidatTuples)
                count++;

            IList<String> res = new List<String>();
            res.Add("" + count);
            result.Add(res);
            return result;
        }

    }
}
