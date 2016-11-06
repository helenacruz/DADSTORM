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
        public IList<string> CustomOperation(IList<string> unique_tuples, IList<string> candidat_tuples,IList<string> op_specs)
        {
            int field_number;
            if (!Int32.TryParse(op_specs[0], out field_number))
                throw new WrongOpSpecsException("Unique Operator Specification need to be an integer.");

            foreach (string candidat_tuple in candidat_tuples)
            {
                foreach(string unique_tuple in unique_tuples) { 
                    string[] splited_queue = unique_tuple.Split(',');
                    string[] splited_candidat = candidat_tuple.Split(',');

                    if (!splited_candidat[field_number - 1].Equals(splited_queue[field_number - 1]))
                            unique_tuples.Add(unique_tuple);
                         
                }
            }
            return unique_tuples;
        }

    }
}
