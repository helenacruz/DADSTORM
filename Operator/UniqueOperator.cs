using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operator
{
    class UniqueOperator : Operator
    {
        private int field_number;

        public UniqueOperator (string id, List<string> sources, string rep_fact, string routing, List<string> urls, int port, int field_number) : base (id, sources, rep_fact, routing, urls, port)
        {
            this.field_number = field_number;
        }

        public override void startOP()
        {
            if (!Sources[0].Contains("tcp://")){ 
                string line;
                StreamReader file;

                file = new StreamReader(Sources[0]);

                while ((line = file.ReadLine()) != null)
                {
                    queueTuple(line.Split(' ').ToList());
                }
                Console.WriteLine("Readed all Tuples from a file.");
                processQueue();
                Console.WriteLine("Processed all Tuples.");
            }
            else
                Console.WriteLine("Waiting tuples from an operator");
        }
        public override void processQueue()
        {
           
            List<List<string>> unique = new List<List<string>>();
            foreach (List<string> tuple_to_add in Queue)
            {
                foreach (List<string> unique_tuple in unique)
                    if (!tuple_to_add[field_number - 1].Equals(unique_tuple[field_number - 1]))
                    {
                        unique.Add(tuple_to_add);

                        string tuple="";
                        foreach (string s in tuple_to_add)
                            tuple+= s;
                        Console.WriteLine(tuple);
                    }
            }
            if (Receiver != null)
            {
                Receiver.processTuples(unique);
                Queue.Clear();
            }
              
        }

    }
}
