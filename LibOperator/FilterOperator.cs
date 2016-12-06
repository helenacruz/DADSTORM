using Shared_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class FilterOperator : IOperator
    {
        public IList<IList<string>> CustomOperation(IList<IList<string>> candidatTuples, IList<string> opSpecs)
        {
            IList<IList<string>> result = new List<IList<string>>();
            int field_number;
            if (opSpecs.Count!=3 || opSpecs[2].Equals(""))
                throw new WrongOpSpecsException("Filter Operator Specification needs 3 arguments.");
            if (!Int32.TryParse(opSpecs[0], out field_number))
                throw new WrongOpSpecsException("Filter Operator Specification need to be an integer in first field.");
            if(!opSpecs[1].Equals("=") && !opSpecs[1].Equals("<") && !opSpecs[1].Equals(">"))
                throw new WrongOpSpecsException("Filter Operator Specification need to be = or < or >.");

            foreach (IList<string> candidat_tuple in candidatTuples)
            {
                if (opSpecs[1].Equals("="))
                {
                    if (candidat_tuple[field_number - 1].Equals(opSpecs[2]))
                        result.Add(candidat_tuple);
                }    
                else if (opSpecs[1].Equals("<"))
                {
                    Version a = new Version(candidat_tuple[field_number - 1]);
                    Version b = new Version(opSpecs[2]);
                    if (a<b)
                        result.Add(candidat_tuple);
                }
                else
                {
                    Version a = new Version(candidat_tuple[field_number - 1]);
                    Version b = new Version(opSpecs[2]);
                    if (a > b)
                        result.Add(candidat_tuple);
                }   
                  
            }

            return result;
        }

    }
}
