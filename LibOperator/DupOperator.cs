using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class DupOperator : IOperator
    {
        public IList<IList<string>> CustomOperation(IList<IList<string>> candidatTuples, IList<string> opSpecs)
        {
            if (opSpecs != null)
                throw new WrongOpSpecsException("Dup Operator Specification has not arguments.");
            return candidatTuples;
        }

    }
}
