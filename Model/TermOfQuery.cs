using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineP
{
    public class TermOfQuery
    {
        private string t_term;
        private int t_numOfShows;
        private int t_numOfDocs;
        //private int t_numOfRelevantDocs;
        //public string[] t_docTf; //name of document and times the term there 
        private int qfi;// num of times the term appear in the query
        public Dictionary<string, int> DocsOfTerm;
        public List<string> l;

        public TermOfQuery(string term)
        {
            t_term = term;
            qfi = 1;
            DocsOfTerm = new Dictionary<string, int>();
        }

        /// <summary>
        /// property of the term
        /// </summary>
        public string Term
        {
            get { return t_term; }
            set { t_term = value; }
        }

        /// <summary>
        /// property to NumOfShows of the term
        /// </summary>
        public int NumOfShows
        {
            get { return t_numOfShows; }
            set { t_numOfShows = value; }
        }

        /// <summary>
        /// property to NumOfDocs of the term
        /// </summary>
        public int NumOfDocs
        {
            get { return t_numOfDocs; }
            set { t_numOfDocs = value; }
        }

        /// <summary>
        /// property of the term
        /// </summary>
        public int QFI
        {
            get { return qfi; }
            set { qfi = value; }
        }
    }
}
