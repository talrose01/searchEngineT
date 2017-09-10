using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineP.Model
{
    public class Query
    {
        public Dictionary<string, TermOfQuery> terms;
        public SortedDictionary<double, HashSet<string>> scoresOfDocs;
        private int q_size;
        private string q_query;
        public Query(string query)
        {
            q_query = query;
            terms = new Dictionary<string, TermOfQuery>();
            scoresOfDocs = new SortedDictionary<double, HashSet<string>>();
        }

        /// <summary>
        /// property of the size
        /// </summary>
        public int Size
        {
            get { return q_size; }
            set { q_size = value; }
        }
        /// <summary>
        /// property of the size
        /// </summary>
        public string QueryS
        {
            get { return q_query; }
            set { q_query = value; }
        }

        public Dictionary<string, double> GetTop50()
        {
            Dictionary<string, double> scores = new Dictionary<string, double>();
            for(int i= scoresOfDocs.Count-1; i>=0; i--)
            {
                foreach(string nameOfDoc in scoresOfDocs.Values.ElementAt(i))
                {
                    if (scores.Count < 50)
                    {
                        scores.Add(nameOfDoc, scoresOfDocs.Keys.ElementAt(i));
                    }
                    else return scores;
                }
            }
            return scores;
        }
    }
}
