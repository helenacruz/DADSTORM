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
        private IList<string> uniqueTuples = new List<string>();

        public IList<string> CustomOperation(IList<string> candidatTuples,IList<string> opSpecs)
        {
            IList<string> result = new List<string>();
            int field_number;
            if (!Int32.TryParse(opSpecs[0], out field_number))
                throw new WrongOpSpecsException("Unique Operator Specification need to be an integer.");
            foreach (string candidat_tuple in candidatTuples)
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
                        string[] splited_queue = uniqueTuples[i].Split(',');
                        string[] splited_candidat = candidat_tuple.Split(',');

                        if (splited_candidat[field_number - 1].Equals(splited_queue[field_number - 1]))
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
