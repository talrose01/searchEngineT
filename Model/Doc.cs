using System.Collections.Generic;

namespace SearchEngineP.Model
{
    public class Doc
    {
        private string d_name; //name of the document
        string d_date; //date of the document
        private string d_sourceLanguage; //the source language

        int max_tf; //number of shows of the value appears the most times
        string ValueMax_tf; //the value appears the most times
        public int numOfTerms; // num of terms in the document
        int numOfUniqueWords; //num of unique words

        public Dictionary<string, int> TermsInDoc; //term and num of times it appears 

        /// <summary>
        /// create a new document
        /// </summary>
        /// <param name="name"> document name</param>
        public Doc(string name)
        {
            d_name = name;
            d_sourceLanguage = "";
            d_date = "";
            max_tf = 0;
            numOfUniqueWords = 0;
            numOfTerms = 0;
            TermsInDoc = new Dictionary<string, int>();
        }

        /// <summary>
        /// add a term to dic
        /// </summary>
        /// <param name="term">term to add</param>
        public void addTermToDocDic(string term)
        {
            int maxTF = 1;
            //case its a new show of this term
            if (!TermsInDoc.ContainsKey(term))
            {
                TermsInDoc.Add(term, 1);
                if (max_tf == 0)
                {
                    //case it is the first term that added to the dictinary
                    max_tf = 1;
                    ValueMax_tf = term;
                }
                numOfUniqueWords++;
            }
            //case it is already in the dic
            else
            {
                TermsInDoc[term]++;
                maxTF = TermsInDoc[term];
                if (maxTF > max_tf)
                {
                    max_tf = maxTF;
                    ValueMax_tf = term;
                }

            }
            numOfTerms++;
        }

        /// <summary>
        /// get name
        /// </summary>
        public string Name
        {
            get { return d_name; }
            set { d_name = value; }
        }

        /// <summary>
        /// source language
        /// </summary>
        public string SourceLanguage
        {
            get { return d_sourceLanguage; }
            set { d_sourceLanguage = value; }
        }

        /// <summary>
        /// date of document
        /// </summary>
        public string Date
        {
            get { return d_date; }
            set { d_date = value; }
        }

        /// <summary>
        /// string of all the details of the document
        /// </summary>
        /// <returns>string of all information</returns>
        public string DocDetails()
        {
            string s = d_name + "¥"+ d_sourceLanguage+ "€" + numOfTerms + "€" + numOfUniqueWords + "€" + ValueMax_tf + "€" + max_tf + "€" + d_date;
            //foreach (KeyValuePair<string, int> t in TermsInDoc)
            //{
            //    s = s + "€" + t.Key + " " + t.Value;
            //}
            return s;
        }
    }
}
