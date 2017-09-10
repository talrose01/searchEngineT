using SearchEngineP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngineP
{
    public class Searcher
    {
        //public static Dictionary<string, int> relevantDocsOfQuery;
        public static HashSet<string> stopWordsDic; //hold the stopwords
        public static string pathPost; //address until "\\Docs..."
        public static string pathOfDoc;
        //public static HashSet<TermOfQuery> queryTerms; //name of document and times the term there 
        public static List<string> languages;
        public static bool isLan;
        public static bool isStem; // hold if is stemming
        public static StemmerInterface stemmer;
        static public int queryNumOfRelevantDocs;
        public static string path;
        private string pathOfQuries;
        public static Dictionary<string, string[]> DocsDetails;
        public static HashSet<Query> Quries;
        //public static Dictionary<string, Dictionary<string, TermOfQuery>> Quries;
        //public static Dictionary<string, double> QuriesScores;
        /// <summary>
        /// a single qurey builder
        /// </summary>
        /// <param name="path"></param>
        public Searcher(string pathPosting)
        {
            path = pathPosting;
            stemmer = new Stemmer();
            //QuriesScores = new Dictionary<string, double>();
            Quries = new HashSet<Query>();
            BuildPaths();
        }

        private void BuildPaths()
        {
            if (isStem)
            {
                pathPost = path + "\\SPost";
                pathOfDoc = path + "\\SPostDoc.txt";
            }
            else
            {
                pathPost = path + "\\Post";
                pathOfDoc = path + "\\PostDoc.txt";
            }
        }

        /// <summary>
        /// searcher of a file with quries
        /// </summary>
        /// <param name="path"></param>
        /// <param name="language"></param>
        /// <param name="stem"></param>
        public Dictionary<string, Dictionary<string, double>> SearchFileOfQueries(string path, List<string> language, bool stem)
        {
            pathOfQuries = path;
            isStem = stem;
            GetLanguage(language);
            string[] quries;
            try
            {
                string data = System.IO.File.ReadAllText(path);
                quries = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            catch (Exception e)
            {
                throw e;
            }
            Thread[] threads = new Thread[quries.Length];
            for (int i = 0; i < quries.Length; i++)
            {
                threads[i] = new Thread((getQuery) =>
                {
                    Query q = new Query(quries[i]);
                    Quries.Add(q);
                    ParseQuery pq = new ParseQuery();
                    pq.ParseTheQuery(ref q);
                    Ranker r = new Ranker();
                    r.scoreDocs(ref q);
                });
                threads[i].Start();
            }
            for (int i = 0; i < quries.Length; i++)
            {
                threads[i].Join();
            }
            Dictionary<string,Dictionary<string, double>> l= new Dictionary<string, Dictionary<string, double>>();
            foreach (Query qu in Quries)
            {
                l.Add(qu.QueryS, qu.GetTop50());
            }
            return l;
        }

        public Dictionary<string, double> SearchSingleQuery(string query, List<string> language, bool stem)
        {
            isStem = stem;
            GetLanguage(language);
            Query q = new Query(query);
            Quries.Add(q);
            ParseQuery pq = new ParseQuery();
            pq.ParseTheQuery(ref q);
            Ranker r = new Ranker();
            r.scoreDocs(ref q);
            Dictionary<string, double> scores = q.GetTop50();
            return scores;
        }

        public static void ReadDocs()
        {
            DocsDetails = new Dictionary<string, string[]>();
            try
            {
                using (Stream s = new FileStream(pathOfDoc, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(s))
                    {
                        while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            string line = br.ReadString();
                            string[] name = line.Split(new char[] { '¥' });
                            string[] details = name[1].Split(new char[] { '€' });
                            DocsDetails.Add(name[0], details);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            //List<string> lan = new List<string>();
            //foreach(KeyValuePair<string, string[]> s in DocsDetails)
            //{
            //    if(!lan.Contains(s.Value[0]))
            //        lan.Add(s.Value[0]);
            //}
        }

        //public static void createRelevateDic()
        //{
        //    relevantDocsOfQuery = new Dictionary<string, int>();
        //    relevantDocsOfQuery["space program"] = 92;
        //    relevantDocsOfQuery["water pollution"] = 228;
        //    relevantDocsOfQuery["genetic engineering"] = 55;
        //    relevantDocsOfQuery["international terrorists"] = 324;
        //    relevantDocsOfQuery["impact of government regulated grain farming on international"] = 808;
        //    relevantDocsOfQuery["real motives for murder"] = 584;
        //    relevantDocsOfQuery["airport security"] = 21;
        //    relevantDocsOfQuery["wildlife extinction"] = 75;
        //    relevantDocsOfQuery["piracy"] = 94;
        //    relevantDocsOfQuery["nobel prize winners"] = 22;
        //    relevantDocsOfQuery["oceanographic vessels"] = 60;
        //    relevantDocsOfQuery["schengen agreement"] = 19;
        //    relevantDocsOfQuery["three gorges project"] = 28;
        //    relevantDocsOfQuery["robotic technology"] = 45;
        //    relevantDocsOfQuery["king hussein, peace"] = 183;
        //}

        private void GetLanguage(List<string> language)
        {
            if (language.Count == 0)
                isLan = false;
            else
            {
                languages = language;
                isLan = true;
            }
        }

        public static void SearchWordOfQuery(string word, Query query)
        {
            lock (Quries)
            {
                string pathD;
                string pathP;
                if (!query.terms.ContainsKey(word))
                {
                    TermOfQuery tq = new TermOfQuery(word);
                    query.terms.Add(word, tq);
                    if ((word[0] >= 'a') && (word[0] <= 'z'))
                    {
                        pathD = pathPost + "D" + word[0] + ".txt"; //create the name of the word's file 
                        pathP = pathPost + "P" + word[0] + ".txt"; //create the name of the word's file                                             
                    }
                    else
                    {
                        pathD = pathPost + "D" + "numAndRules.txt";
                        pathP = pathPost + "P" + "numAndRules.txt";
                    }
                    int numOfLine = GetInfoOfTerm(ref pathD, ref tq); //read the dictionary file
                    GetDocsOfTerm(ref pathP, ref tq, numOfLine); //read the posting file
                }
                else
                {
                    query.terms[word].QFI++;
                }
            }
        }

        /// <summary>
        /// get the dic terms of all the files
        /// </summary>
        /// <param name="path"> path the path to the file</param>
        private static int GetInfoOfTerm(ref string path, ref TermOfQuery word)
        {
            int numOfLine = 0;
            try
            {
                using (Stream s = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(s))
                    {
                        while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            string line = br.ReadString(); //read the line
                            string[] fields = line.Split(new char[] { '¥' });
                            if (fields[0] == word.Term) //the term is the wanted term
                            {
                                string[] split = fields[1].Split(new char[] { '€' });
                                int shows, docs;
                                if (int.TryParse(split[0], out shows)) //get the num of shows
                                    word.NumOfShows = shows;
                                if (int.TryParse(split[1], out docs)) //get the num of docs
                                    word.NumOfDocs = docs;
                                break;
                            }
                            else
                            {
                                numOfLine++; //move on to the next line
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return numOfLine;

        }

        /// <summary>
        /// get the posting details of the term
        /// </summary>
        /// <param name="path"></param>
        /// <param name="word"></param>
        /// <param name="numOfLine"></param>
        private static void GetDocsOfTerm(ref string path, ref TermOfQuery word, int numOfLine)
        {
            int counter = 0; //counter of the line
            try
            {
                using (Stream s = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(s))
                    {
                        while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            if (counter == numOfLine) //we get to the wanted line
                            {
                                string line = br.ReadString(); //read the line
                                                               //to check if can insert to file the object TERM
                                string[] details = line.Split(new char[] { '¥', '€', '|' }, StringSplitOptions.RemoveEmptyEntries); //split the docs details
                                                                                                                                    //to check
                                for (int i = 1; i < details.Length; i += 2)
                                {
                                    int num;
                                    int.TryParse(details[i + 1], out num);
                                    if (!word.DocsOfTerm.ContainsKey(details[i]))
                                        word.DocsOfTerm.Add(details[i], num);
                                }
                                break;
                            }
                            else //move on to the next line
                            {
                                counter++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// create the stop words dictionary
        /// </summary>
        /// <param name="path"> the path of the file of the stop words</param>
        public static void createStopWordsDic()
        {
            stopWordsDic = new HashSet<string>();
            try
            {
                string data = System.IO.File.ReadAllText(path + @"\stop_words.txt");
                string[] stopwords = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < stopwords.Length; i++)
                {
                    stopWordsDic.Add(stopwords[i]);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
