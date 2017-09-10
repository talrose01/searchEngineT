using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SearchEngineP.Model
{
    public class ReadFile
    {
        private volatile int numOfDocs; // num of the documents      
        private string path; //path fo the file of corpus and stopword
        private string pathPosting; //path to save the posting files
        private const int NumOfThreads = 15; // threads run parallel
        private Indexer indexer; //indexer object
        private Mutex countDocM;// mutex for the doc counter
        public static HashSet<string> stopWordsDic; //hold the stopwords
        private ConcurrentQueue<string> namesOfFiles; //hold the names of the files to read the documents from
        private ConcurrentQueue<Parse> parseQueue; //queue of parses
        ConcurrentQueue<Dictionary<string, Term>> termsOfDocsOfFile; //queue hold dictionaries of terms
        ConcurrentQueue<HashSet<string>> docInfo; //queue hold strings of doc informations
        public List<string> languageList; //list of the lanuages of the documents
        public static StemmerInterface stemmer;
        public static int totalNumOfWords;
        public static Dictionary<string, Dictionary<string, int>> timesOfN;
        public static Dictionary<string, string[]> top5;
        //event connect with gui
        public delegate void ModelFunc(int num, string content);
        public event ModelFunc ModelChanged;

        /// <summary>
        /// create a new object of readfile
        /// </summary>
        /// <param name="path">the path to the corpus</param>
        /// <param name="pathPosting">the path to the posting</param>
        /// <param name="stemming">tell if use stem</param>
        public ReadFile(string path, string pathPosting, bool stemming)
        {
            countDocM = new Mutex();
            timesOfN = new Dictionary<string, Dictionary<string, int>>();
            top5 = new Dictionary<string, string[]>();
            // start parameters
            this.path = path;
            this.pathPosting = pathPosting;
            numOfDocs = 0;
            totalNumOfWords = 0;
            Parse.isStem = stemming;
            Indexer.pathPosting = pathPosting;
            Indexer.isStem = stemming;
            ReadFile.stemmer = new Stemmer();
            // start data bases
            languageList = new List<string>();
            indexer = new Indexer();

            // call to create functions
            createStopWordsDic();
            getFilesPaths();
            startQueues();
            createParse();
        }

        /// <summary>
        /// initialite the queue of the terms information and the docs information
        /// </summary>
        private void startQueues()
        {
            termsOfDocsOfFile = new ConcurrentQueue<Dictionary<string, Term>>();
            for (int i = 0; i < NumOfThreads; i++)
            {
                Dictionary<string, Term> dic = new Dictionary<string, Term>();
                termsOfDocsOfFile.Enqueue(dic);
            }

            docInfo = new ConcurrentQueue<HashSet<string>>();
            for (int i = 0; i < NumOfThreads; i++)
            {
                HashSet<string> list = new HashSet<string>();
                docInfo.Enqueue(list);
            }
        }

        /// <summary>
        /// create the stop words dictionary
        /// </summary>
        /// <param name="path"> the path of the file of the stop words</param>
        private void createStopWordsDic()
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

        /// <summary>
        /// returns the all paths of files of a given path of directory
        /// </summary>
        private void getFilesPaths()
        {
            namesOfFiles = new ConcurrentQueue<string>();
            try
            {
                string[] filesNames = Directory.GetFiles(path + @"\corpus");
                foreach (string file in filesNames)
                {
                    namesOfFiles.Enqueue(file);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// create the queue of the parses
        /// </summary>
        private void createParse()
        {
            Parse.createMonthDic();
            parseQueue = new ConcurrentQueue<Parse>();
            for (int i = 0; i < NumOfThreads; i++)
            {
                Parse parse = new Parse();
                parseQueue.Enqueue(parse);
            }
        }

        /// <summary>
        /// start the threads that reads the docs files
        /// </summary>
        public void startThreads()
        {
            //start the watch of time the all operations: read the files, parsing and indexing
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //event that the indexing started
            ModelChanged(1, "Start Indexing");

            //int countToWriteToFile = 1;

            while (namesOfFiles.Count > 0)
            {
                //num of threads that running
                int threadsCounter = 0;
                //array of threads that parse in parall the documents
                Thread[] threads = new Thread[NumOfThreads];
                for (int i = 0; i < NumOfThreads; i++)
                {
                    string fileName = "";
                    //get the file to work on
                    if (namesOfFiles.TryDequeue(out fileName))
                    {
                        threadsCounter++;
                        threads[i] = new Thread((splitDocs) =>
                        {
                            //dictionary of terms from the queue of dictionarys
                            Dictionary<string, Term> termsToAdd;
                            termsOfDocsOfFile.TryDequeue(out termsToAdd);

                            //hashset from the queue of listDocuments
                            HashSet<string> listDoc;
                            if (!docInfo.TryDequeue(out listDoc))
                                listDoc = new HashSet<string>();
                            //start the read doc process
                            startReadDocs(fileName, ref termsToAdd, ref listDoc);

                            //return the dictionary and the set to there queues
                            termsOfDocsOfFile.Enqueue(termsToAdd);
                            docInfo.Enqueue(listDoc);
                        }

                                           );
                        threads[i].Start();
                    }
                }

                //wait to all threads to finish parsing
                for (int i = 0; i < threadsCounter; i++)
                {
                    threads[i].Join();
                }

                //merege the all information about the terms of all the dictionaries
                Dictionary<string, Term> uTermsToAdd = new Dictionary<string, Term>();
                HashSet<string> uListDoc = new HashSet<string>();
                for (int i = 0; i < NumOfThreads; i++)
                {
                    Dictionary<string, Term> termsToAdd;
                    if (termsOfDocsOfFile.TryDequeue(out termsToAdd))
                    {
                        foreach (var item in termsToAdd)
                        {
                            if (uTermsToAdd.ContainsKey(item.Key))
                            {
                                uTermsToAdd[item.Key].NumOfDocuments += item.Value.NumOfDocuments;
                                uTermsToAdd[item.Key].NumOfShows += item.Value.NumOfShows;
                                foreach (var docTf in item.Value.t_docTf)
                                {
                                    uTermsToAdd[item.Key].t_docTf.Add(docTf.Key, docTf.Value);
                                }
                            }
                            else
                                uTermsToAdd.Add(item.Key, item.Value);
                        }
                    }

                    //merege the all information about the docs all the threads
                    HashSet<string> listDoc;
                    while (docInfo.TryDequeue(out listDoc))
                    {
                        foreach (string s in listDoc)
                        {
                            uListDoc.Add(s);
                        }
                    }

                    //initialize new dic and hash to insert to the queues
                    termsToAdd = new Dictionary<string, Term>();
                    listDoc = new HashSet<string>();

                    termsOfDocsOfFile.Enqueue(termsToAdd);
                    docInfo.Enqueue(listDoc);

                }

                //write the all current documents and terms to disk
                indexer.WritePostingFile(ref uTermsToAdd);
                indexer.WriteDocFile(ref uListDoc);
            }
            GetTop5();
            //stop watch
            watch.Stop();
            //get time in format of hours, secons, minutes
            var secondsTime = watch.Elapsed;

            //get num of terms
            getTermsToDictionaryR();
            double avgLength = totalNumOfWords / 130471;
            ModelChanged(1, "Indexing Your Data Set  Finiahed!\nNumber Of Docs That Indexing: " + numOfDocs + "\nNumber of Uniq Terms: " + Indexer.numOfUniqeWords + "\nTotal Time Of Indexing Process: " + secondsTime + "Avg Num Of Words: " + avgLength + ".\n");
        }

        private void GetTop5()
        {
            
            foreach (KeyValuePair<string, Dictionary<string, int>> value in timesOfN)
            {
                //string s1, s2, s3, s4, s5;
                top5[value.Key] = new string[5];
                Dictionary<string, int> termN = new Dictionary<string, int>();
                //int c = 1;
                foreach (KeyValuePair<string, int> n in value.Value)
                {
                    if (termN.Count < 5)
                    {
                        termN.Add(n.Key, n.Value);
                    }
                    else
                    {
                        int min = termN.Values.ElementAt(0);
                        string minK = termN.Keys.ElementAt(0);
                        foreach (KeyValuePair<string, int> k in termN)
                        {
                            if (k.Value < min)
                            {
                                min = k.Value;
                                minK = k.Key;
                            }
                        }

                        termN.Remove(minK);
                        termN[n.Key] = n.Value;
                    }
                }
                int j = 0;
                foreach (KeyValuePair<string, int> k in termN)
                {
                    top5[value.Key][j] = k.Key;
                    j++;
                }
            }
            WriteDocOFNToFile();
        }

        private void WriteDocOFNToFile()
        {
            string pathToWrite = pathPosting + "\\DocsInfo.txt";
            using (StreamWriter sw = new StreamWriter(pathToWrite, true))
            {
                foreach(KeyValuePair<string, string[]> t in top5)
                {
                    string line = t.Key + "¥";
                    foreach (string s in t.Value)
                    {
                        line = line + s + "€";
                    }
                    sw.WriteLine(line);
                }                       
            }
        }
        /// <summary>
        /// function to give a string clean from spaces in the sides
        /// </summary>
        /// <param name="stringToCut"> string with spaces</param>
        /// <returns><string without spafes/returns>
        private string cleanSpaces(ref string stringToCut)
        {
            while (stringToCut[0] == ' ')
            {
                stringToCut = stringToCut.Substring(1);
            }
            while (stringToCut[stringToCut.Length - 1] == ' ')
            {
                stringToCut = stringToCut.Substring(0, stringToCut.Length - 1);
            }
            return stringToCut;
        }

        /// <summary>
        /// create the docs from from the corpus
        /// </summary>
        /// <param name="fileName">name of the file</param>
        /// <param name="termsToAdd">the finish sorted dic</param>
        /// <param name="listDoc">list to add the docs</param>

        private void startReadDocs(string fileName, ref Dictionary<string, Term> termsToAdd, ref HashSet<string> listDoc)
        {
            HtmlAgilityPack.HtmlDocument fileDoc = new HtmlAgilityPack.HtmlDocument();
            //load the html from the file
            fileDoc.Load(fileName);
            //split the file to docs and store them in docNodes
            HtmlAgilityPack.HtmlNode[] docNodes = fileDoc.DocumentNode.SelectNodes("//doc").ToArray();
            foreach (HtmlAgilityPack.HtmlNode docNode in docNodes) //iterate on the docs
            {
                countDocM.WaitOne();
                numOfDocs++;
                countDocM.ReleaseMutex();
                //get the doc name
                string docName = docNode.ChildNodes["docno"].InnerText;
                docName = cleanSpaces(ref docName);

                //get the doc date
                string docDate = "";
                foreach (HtmlAgilityPack.HtmlNode n in docNode.SelectNodes(".//date1"))
                {
                    docDate = n.InnerText;
                    docDate = cleanSpaces(ref docDate);
                }

                //get the doc language
                string docLanguage = "";
                try
                {
                    docLanguage = docNode.SelectSingleNode("//*[@p='105']").InnerText;
                    docLanguage = cleanSpaces(ref docLanguage);
                }
                catch { }

                if (docLanguage != "")
                {
                    if (!languageList.Contains(docLanguage))
                        languageList.Add(docLanguage);
                }

                //get the doc text
                string docText = "";
                string docTextWithTab = docNode.ChildNodes["text"].InnerText;

                string[] sep = new string[] { "[Text]" };
                string[] nameAndT = docTextWithTab.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (nameAndT.Length == 1)
                { // if there is just the text
                    docText = nameAndT[0];
                }
                else
                {
                    docText = nameAndT[1];
                }

                Parse p;
                //Check if there is an available Parse in the queue
                if (!parseQueue.TryDequeue(out p))
                    p = new Parse();

                //send the document details
                p.insertDocInfo(ref docName, ref docDate, ref docLanguage);
                p.startParsing(ref docText, ref termsToAdd, ref listDoc);

                //return the parse to the queue
                parseQueue.Enqueue(p);
            }
        }

        /// <summary>
        /// call the indexer to build the dicionery of the term in the memory 
        /// </summary>
        /// <param name="stemming">true if stem</param>
        public void getTermsToDictionaryR()
        {
            indexer.getTermsToDictionary();
        }

        /// <summary>
        /// call the indexer to display the dictionary
        /// </summary>
        /// <returns>the dic in string</returns>
        public string getDisplayR()
        {
            return indexer.getDisplayR();
        }

    }
}