using SearchEngineP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Word;
using WordApplication = Microsoft.Office.Interop.Word.Application;
//using Microsoft.Office.Interop.Outlook;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineP
{
    public class Ranker
    {
        //public SortedDictionary<double, string> scoresOfDocs;
        public static Dictionary<string, TermOfQuery> queryTerms; //name of document and times the term there 
        public HashSet<string> namesOfDocs;
        Query r_query;
        double k1, k2, K, b, ri;
        double R;
        double avgdl;
        int N; //num of documents 
        //double TotalScoreOfQuery;

        /// <summary>
        /// create a new ranker
        /// </summary>
        public Ranker()
        {
            N = 130471;
            avgdl = 251; //to check
            ri = 0;
            k1 = 1.2;
            k2 = 1;
            b = 0.75;
            R = 0;
            //TotalScoreOfQuery = 0;
            namesOfDocs = new HashSet<string>();
        }

        /// <summary>
        /// score docs of query
        /// </summary>
        /// <param name="query"></param>
        public void scoreDocs(ref Query query)
        {
            r_query = query;
            queryTerms = query.terms;
            //if (queryTerms.Count < 2)
            //    ri = R; //to get  
            if (Searcher.isLan)
            {
                scoreDocsWithLan();
            }
            else
            {
                scoreDocsWithOutLan();
            }

            //query.sd= scoresOfDocs;
        }

        /// <summary>
        /// score just docs with the given languages
        /// </summary>
        private void scoreDocsWithLan()
        {
            foreach (KeyValuePair<string, TermOfQuery> term in queryTerms)
            {
                foreach (KeyValuePair<string, int> doc in term.Value.DocsOfTerm) //move on all the docs
                {
                    if (Searcher.languages.Contains(Searcher.DocsDetails[doc.Key][0]))
                    {
                        if (!namesOfDocs.Contains(doc.Key))
                        {
                            ScoreBM12(doc.Key);
                            namesOfDocs.Add(doc.Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// score all the docs
        /// </summary>
        private void scoreDocsWithOutLan()
        {
            foreach (KeyValuePair<string, TermOfQuery> term in queryTerms)
            {
                foreach (KeyValuePair<string, int> doc in term.Value.DocsOfTerm) //move on all the docs
                {
                    if (!namesOfDocs.Contains(doc.Key))
                    {
                        ScoreBM12(doc.Key);
                        namesOfDocs.Add(doc.Key);
                    }
                }
            }
        }

        /// <summary>
        /// score ths relevant documents by BM12 
        /// </summary>
        private void ScoreBM12(string nameOfDoc)
        {
            double Totalscore = 0;
            int dl; //the doc's length
            int.TryParse(Searcher.DocsDetails[nameOfDoc][1], out dl);
            foreach (KeyValuePair<string, TermOfQuery> t in queryTerms)
            {
                //calculate the score BM12 for each term
                double score = 0; //score for a word in the query
                int ni = t.Value.NumOfDocs; //num of docs containing the word
                int fi = 0;//frequency of the word in the doc
                if (t.Value.DocsOfTerm.ContainsKey(nameOfDoc))
                    fi = t.Value.DocsOfTerm[nameOfDoc];
                int qfi = t.Value.QFI;       

                K = k1 * ((1 - b) + b * (dl / avgdl));
                double mul1 = ((ri + 0.5) / (R - ri + 0.5)) / ((ni - ri + 0.5) / (N - ni - R + ri + 0.5));
                double mul2 = (((k1 + 1) * fi) / (K + fi));
                double mul3 = (((k2 + 1) * qfi) / (k2 + qfi));
                score = mul1 * mul2 * mul3;
                score = Math.Log(score, 2);
                //score = Math.Log(score);
                Totalscore = Totalscore + score;
            }
            if (r_query.scoresOfDocs.ContainsKey(Totalscore)) //the score is allready exsits
            {
                r_query.scoresOfDocs[Totalscore].Add(nameOfDoc);
            }
            else if (r_query.scoresOfDocs.Count < 50) // there aren't 50 docs yet
            {
                r_query.scoresOfDocs.Add(Totalscore, new HashSet<string>());
                r_query.scoresOfDocs[Totalscore].Add(nameOfDoc);
            }
            else // there more than 50 docs
            {
                if (Totalscore > r_query.scoresOfDocs.Keys.First()) // the doc need to insert top 10
                {
                    r_query.scoresOfDocs.Remove(0); //remove the doc with the lowest score
                    r_query.scoresOfDocs.Add(Totalscore, new HashSet<string>()); //insert the new doc
                    r_query.scoresOfDocs[Totalscore].Add(nameOfDoc);
                }
            }
        }

    }
}
