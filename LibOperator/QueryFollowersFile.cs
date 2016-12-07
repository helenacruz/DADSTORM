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
        public IList<IList<string>> getFollowers(IList<IList<string>> candidatTuples)
        {
            IList<IList<string>> result = new List<IList<string>>();
            StreamReader file = new StreamReader("../../../Input/followers.dat");
            IList<IList<string>> followers_tuples = new List<IList<string>>();
            string line = "";
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("%"))
                {
                    string[] splited_follower = removeWhiteSpaces(line).Split(',');
                    IList<String> res = new List<String>();
                    foreach (string s in splited_follower)
                        res.Add(s);
                    followers_tuples.Add(res);
                }
            }
            foreach (IList<string> candidat_tuple in candidatTuples)
            {
                foreach (IList<string> followers_tuple in followers_tuples)
                {
                    if (candidat_tuple[1].Equals(followers_tuple[0]))
                        result.Add(followers_tuple);
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