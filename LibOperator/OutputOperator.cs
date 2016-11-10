using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class OutputOperator :IOperator 
    {
        public IList<string> CustomOperation(IList<string> candidatTuples, IList<string> opSpecs)
        {
            string outputFile = @"../../../Output.txt";

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(outputFile, true))
            {
                foreach (string line in candidatTuples)
                {
                    file.WriteLine(line);
                }
            }
            IList<IList<string>> result = new List<IList<string>>();
            return candidatTuples;
        }

    }
}
