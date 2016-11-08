using Shared_Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibOperator
{
    public class QueryFollowersFile
    {
        public IList<string> getFollowers(IList<string> candidatTuples)
        {
            IList<string> result = new List<string>();
            StreamReader file = new StreamReader("../../../Input/followers.dat");
            List<string> followers_tuples = new List<string>();
            string line = "";
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("%"))
                {
                    followers_tuples.Add(removeWhiteSpaces(line));
                }
            }
            foreach (string candidat_tuple in candidatTuples)
            {
                string[] splited_candidat = candidat_tuple.Split(',');
                foreach (string followers_tuple in followers_tuples)
                {
                    string[] splited_follower = followers_tuple.Split(',');
                    if (splited_candidat[1].Equals(splited_follower[0]))
                        result.Add(candidat_tuple);
                }
            }
                return result;
        }

        private string removeWhiteSpaces(string s)
        {
            bool aceptingSpaces = false;
            string result = "";
            foreach (char c in s)
            {
                if (c == '\"')
                {
                    s += c;
                    aceptingSpaces = !aceptingSpaces;
                }
                if ((c == ' ' && aceptingSpaces) || c != ' ')
                    result += c;
            }
            return result;
        }
    }

}
