using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class UniqueOperator : IOperator
    {
        private IList<IList<string>> uniqueTuples = new List<IList<string>>();

        public IList<IList<string>> CustomOperation(IList<IList<string>> candidatTuples,IList<string> opSpecs)
        {
            IList<IList<string>> result = new List<IList<string>>();
            int field_number;
            if (!Int32.TryParse(opSpecs[0], out field_number))
                throw new WrongOpSpecsException("Unique Operator Specification need to be an integer.");
            foreach (IList<string> candidat_tuple in candidatTuples)
            {
                if (uniqueTuples.Count == 0)
                {
                    result.Add(candidat_tuple);
                    uniqueTuples.Add(candidat_tuple);
                }
                else
                {
                    bool unique = true;
                    for (int i=0; i< uniqueTuples.Count;i++)
                    {
                        if (candidat_tuple[field_number - 1].Equals(uniqueTuples[i][field_number - 1]))
                        {
                            unique = false;
                            break;
                        }
                    }
                    if (unique)
                    {
                        result.Add(candidat_tuple);
                        uniqueTuples.Add(candidat_tuple);
                    }
                }
            }

            return result;
        }

    }
}
