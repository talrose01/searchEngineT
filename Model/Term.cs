using System.Collections.Generic;
using System.Linq;

namespace SearchEngineP.Model
{
    public class Term
    {
        private string t_value; //the term
        private int t_numOfDocuments; //num of documents that includes the term
        private int t_numOfShows; //num of total shows
        public Dictionary<string, int> t_docTf; //name of document and times the term there 
        //public Dictionary<string, string> t_doclanguages;
        //public Dictionary<string, string> t_LengthPfDoc;
        /// <summary>
        /// create a new term
        /// </summary>
        /// <param name="value"></param>
        public Term(string value)
        {
            t_value = value;
            t_numOfDocuments = 0;
            t_numOfShows = 0;
            t_docTf = new Dictionary<string, int>();
            //t_doclanguages = new Dictionary<string, string>();
        }
        /// <summary>
        /// property to the value of the term
        /// </summary>
        public string Value
        {
            get { return t_value; }
            set { t_value = value; }
        }
        /// <summary>
        /// property to NumOfDocuments of the term
        /// </summary>
        public int NumOfDocuments
        {
            get { return t_numOfDocuments; }
            set { t_numOfDocuments = value; }
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
        /// update the doc of the term
        /// </summary>
        /// <param name="docNme">the name of the doc</param>
        public void updateDocShows(string docNme, string language)
        {
            //check if the doc allready has the term
            if (!t_docTf.ContainsKey(docNme))
            {
                t_docTf.Add(docNme, 1);
                //t_doclanguages.Add(docNme, language);
                t_numOfDocuments++;
            }
            else
            {
                t_docTf[docNme]++;
            }
            t_numOfShows++;
        }

        /// <summary>
        /// build the line we sholud add to the dic
        /// </summary>
        /// <returns>the lie to dic</returns>
        public string TermDic()
        {
            string s = t_value + "¥" + NumOfShows + "€" + NumOfDocuments;
            return s;
        }

        /// <summary>
        ///  build the line we sholud add to the posting files
        /// </summary>
        /// <returns>the line to posting</returns>
        public string TermPost()
        {
            string s = "";

            //for (int i = 0; i < t_docTf.Count; i++)
            //{
            foreach (KeyValuePair<string, int> item in t_docTf)
            {
                //string name = t_docTf.ElementAt(i).Key;
                //int num = t_docTf.ElementAt(i).Value;
                //string lan = t_doclanguages.ElementAt(i).Value;
                s = s + item.Key + "€" + item.Value + "|";
            }
            //}
            return s;
        }
    }
}

