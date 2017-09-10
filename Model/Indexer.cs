using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SearchEngineP.Model
{
    class Indexer
    {
        public static string pathPosting; //path of posting 
        private string pathDocS; //path of documents with stemming
        private string pathDoc; //path of document without stemming
        private string pathTermDicS; // path of dictionary with stemming words
        private string pathTermDic; //path of dictionary without stemming
        private string pathTermPostS; //path of posts with stemming
        private string pathTermPost; //path of dictionary without stemming
        private string tempPathD;
        private string tempPathP;
        private Mutex[] m;
        private Mutex mDoc;

        public static bool isStem;
        public SortedDictionary<string, Term>[] dictionaryTerms; //hols the dictionary that read from the file
        public static int numOfUniqeWords;
        public Dictionary<string, string> dic;
        /// <summary>
        /// constructor of index
        /// </summary>
        public Indexer()
        {
            m = new Mutex[27];
            mDoc = new Mutex();
            initMutexArray();
            addDictionarysToArray();
            pathDocS = pathPosting + "\\SDocs\\PostDoc.txt";
            pathDoc = pathPosting + "\\Docs\\PostDoc.txt";
            pathTermDicS = pathPosting + "\\SDocs\\PostD";
            pathTermDic = pathPosting + "\\Docs\\PostD";
            pathTermPostS = pathPosting + "\\SDocs\\PostP";
            pathTermPost = pathPosting + "\\Docs\\PostP";
            tempPathD = pathPosting + "\\Docs\\tempD";
            tempPathP = pathPosting + "\\Docs\\tempP";
            numOfUniqeWords = 0;
        }

        private void initMutexArray()
        {
            for (int i = 0; i < 27; i++)
            {
                m[i] = new Mutex();
            }
        }

        private void addDictionarysToArray()
        {
            dictionaryTerms = new SortedDictionary<string, Term>[27];
            for (int i = 0; i < 27; i++)
            {
                dictionaryTerms[i] = new SortedDictionary<string, Term>();
            }
        }

        /// <summary>
        /// write the docs information to file
        /// </summary>
        /// <param name="docInfo">string of the docs information</param>
        public void WriteDocFile(ref HashSet<string> docInfo)
        {
            string fileName = "";
            if (isStem)
                fileName = pathDocS;
            else
                fileName = pathDoc;
            mDoc.WaitOne();
            try
            {
                using (Stream s = new FileStream(fileName, FileMode.Append))
                {
                    using (BinaryWriter bw = new BinaryWriter(s))
                    {
                        foreach (string doc in docInfo)
                        {
                            bw.Write(doc);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            mDoc.ReleaseMutex();
        }
        //public void WriteDocFile(ref Dictionary<string, HashSet<string>> docInfo)
        //{
        //    string fileName = "";
        //    if (isStem)
        //        fileName = pathDocS;
        //    else
        //        fileName = pathDoc;
        //    mDoc.WaitOne();
        //    WriteDocToGeneralFile(fileName, ref docInfo);
        //    foreach (KeyValuePair<string, HashSet<string>> item in docInfo)
        //    {
        //        fileName = fileName + item.Key + ".txt";
        //        using (Stream s = new FileStream(fileName, FileMode.Append))
        //        {
        //            using (BinaryWriter bw = new BinaryWriter(s))
        //            {
        //                foreach (string doc in item.Value)
        //                {
        //                    bw.Write(doc);
        //                }
        //            }
        //        }
        //    }
        //    mDoc.ReleaseMutex();
        //}

        //private void WriteDocToGeneralFile(string fileName, ref Dictionary<string, HashSet<string>> docInfo)
        //{
        //    string pathFile = fileName + "All.txt";
        //    foreach (KeyValuePair<string, HashSet<string>> item in docInfo)
        //    {
        //        using (Stream s = new FileStream(pathFile, FileMode.Append))
        //        {
        //            using (BinaryWriter bw = new BinaryWriter(s))
        //            {
        //                foreach (string doc in item.Value)
        //                {
        //                    bw.Write(doc);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// write posting terms to files
        /// </summary>
        /// <param name="termsOfFile">terms we should write</param>
        public void WritePostingFile(ref Dictionary<string, Term> terms)
        {
            string pathToAddD;
            string pathToAddP;
            sortDic(ref terms);
            int i = 0;
            foreach (SortedDictionary<string, Term> dicT in dictionaryTerms)
            {
                m[i].WaitOne();
                if (isStem)
                {
                    pathToAddD = pathTermDicS + getNameOfFile(i) + ".txt";
                    pathToAddP = pathTermPostS + getNameOfFile(i) + ".txt";
                }
                else
                {
                    pathToAddD = pathTermDic + getNameOfFile(i) + ".txt";
                    pathToAddP = pathTermPost + getNameOfFile(i) + ".txt";
                }
                if (!File.Exists(pathToAddD))
                {
                    SortedDictionary<string, Term> sd = dicT;
                    CreateNewFile(pathToAddD, pathToAddP, ref sd);
                }
                else
                {
                    SortedDictionary<string, Term> sd = dicT;
                    meregeFiles(pathToAddD, pathToAddP, ref sd);
                }
                m[i].ReleaseMutex();
                i++;
            }
        }

        private string getNameOfFile(int i)
        {
            if (i == 0)
                return "a";
            else if (i == 1)
                return "b";
            else if (i == 2)
                return "c";
            else if (i == 3)
                return "d";
            else if (i == 4)
                return "e";
            else if (i == 5)
                return "f";
            else if (i == 6)
                return "g";
            else if (i == 7)
                return "h";
            else if (i == 8)
                return "i";
            else if (i == 9)
                return "j";
            else if (i == 10)
                return "k";
            else if (i == 11)
                return "l";
            else if (i == 12)
                return "m";
            else if (i == 13)
                return "n";
            else if (i == 14)
                return "o";
            else if (i == 15)
                return "p";
            else if (i == 16)
                return "q";
            else if (i == 17)
                return "r";
            else if (i == 18)
                return "s";
            else if (i == 19)
                return "t";
            else if (i == 20)
                return "u";
            else if (i == 21)
                return "v";
            else if (i == 22)
                return "w";
            else if (i == 23)
                return "x";
            else if (i == 24)
                return "y";
            else if (i == 25)
                return "z";
            else
                return "numAndRules";
        }
        /// <summary>
        /// insert the information to the index files at the first time
        /// </summary>
        /// <param name="pathToAddD"></param>
        /// <param name="pathToAddP"></param>
        /// <param name="termsOfFile">files we should add</param>
        private void CreateNewFile(string pathToAddD, string pathToAddP, ref SortedDictionary<string, Term> termsOfFile)
        {
            try
            {
                using (Stream s1 = new FileStream(pathToAddD, FileMode.Create))
                {
                    using (BinaryWriter bw1 = new BinaryWriter(s1))
                    {
                        using (Stream s2 = new FileStream(pathToAddP, FileMode.Create))
                        {
                            using (BinaryWriter bw2 = new BinaryWriter(s2))
                            {
                                foreach (KeyValuePair<string, Term> term in termsOfFile)
                                {
                                    //write to disk in the first time
                                    bw1.Write(term.Value.TermDic());
                                    bw2.Write(term.Key + "¥" + term.Value.TermPost());
                                }
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
        /// sort the dictionary of terms.
        /// </summary>
        /// <param name="termsOfFile"> unsorted dictionary</param>
        private void sortDic(ref Dictionary<string, Term> terms)
        {
            foreach (KeyValuePair<string, Term> item in terms)
            {
                if (item.Key[0] == 'a')
                    dictionaryTerms[0][item.Key] = item.Value;
                else if (item.Key[0] == 'b')
                    dictionaryTerms[1][item.Key] = item.Value;
                else if (item.Key[0] == 'c')
                    dictionaryTerms[2][item.Key] = item.Value;
                else if (item.Key[0] == 'd')
                    dictionaryTerms[3][item.Key] = item.Value;
                else if (item.Key[0] == 'e')
                    dictionaryTerms[4][item.Key] = item.Value;
                else if (item.Key[0] == 'f')
                    dictionaryTerms[5][item.Key] = item.Value;
                else if (item.Key[0] == 'g')
                    dictionaryTerms[6][item.Key] = item.Value;
                else if (item.Key[0] == 'h')
                    dictionaryTerms[7][item.Key] = item.Value;
                else if (item.Key[0] == 'i')
                    dictionaryTerms[8][item.Key] = item.Value;
                else if (item.Key[0] == 'j')
                    dictionaryTerms[9][item.Key] = item.Value;
                else if (item.Key[0] == 'k')
                    dictionaryTerms[10][item.Key] = item.Value;
                else if (item.Key[0] == 'l')
                    dictionaryTerms[11][item.Key] = item.Value;
                else if (item.Key[0] == 'm')
                    dictionaryTerms[12][item.Key] = item.Value;
                else if (item.Key[0] == 'n')
                    dictionaryTerms[13][item.Key] = item.Value;
                else if (item.Key[0] == 'o')
                    dictionaryTerms[14][item.Key] = item.Value;
                else if (item.Key[0] == 'p')
                    dictionaryTerms[15][item.Key] = item.Value;
                else if (item.Key[0] == 'q')
                    dictionaryTerms[16][item.Key] = item.Value;
                else if (item.Key[0] == 'r')
                    dictionaryTerms[17][item.Key] = item.Value;
                else if (item.Key[0] == 's')
                    dictionaryTerms[18][item.Key] = item.Value;
                else if (item.Key[0] == 't')
                    dictionaryTerms[19][item.Key] = item.Value;
                else if (item.Key[0] == 'u')
                    dictionaryTerms[20][item.Key] = item.Value;
                else if (item.Key[0] == 'v')
                    dictionaryTerms[21][item.Key] = item.Value;
                else if (item.Key[0] == 'w')
                    dictionaryTerms[22][item.Key] = item.Value;
                else if (item.Key[0] == 'x')
                    dictionaryTerms[23][item.Key] = item.Value;
                else if (item.Key[0] == 'y')
                    dictionaryTerms[24][item.Key] = item.Value;
                else if (item.Key[0] == 'z')
                    dictionaryTerms[25][item.Key] = item.Value;
                else
                    dictionaryTerms[26][item.Key] = item.Value;
            }

        }

        /// <summary>
        /// fonction to merge the information in the memory of the program to the index files
        /// </summary>
        /// <param name="pathToAddD"></param>
        /// <param name="pathToAddP"></param>
        /// <param name="termsOfFile">the information we should merge</param>
        private void meregeFiles(string pathToAddD, string pathToAddP, ref SortedDictionary<string, Term> termsOfFile)
        {
            string fileline = "";
            string listline = "";
            string fileline1 = "";
            string listline1 = "";
            string fileTerm = "";
            string listTerm = "";
            string newline = "";
            string newline2 = "";
            //iterator that move on the list to add
            var it = termsOfFile.GetEnumerator();
            try
            {
                using (Stream s1 = new FileStream(tempPathD, FileMode.Create))
                {
                    using (BinaryWriter bw1 = new BinaryWriter(s1))
                    {
                        using (Stream s3 = new FileStream(tempPathP, FileMode.Create))
                        {
                            using (BinaryWriter bw3 = new BinaryWriter(s3))
                            {
                                using (Stream s2 = new FileStream(pathToAddD, FileMode.Open))
                                {
                                    using (BinaryReader br2 = new BinaryReader(s2))
                                    {
                                        using (Stream s4 = new FileStream(pathToAddP, FileMode.Open))
                                        {
                                            using (BinaryReader br4 = new BinaryReader(s4))
                                            {
                                                //get the first line from the file
                                                if (br2.BaseStream.Position != br2.BaseStream.Length)
                                                {
                                                    fileline = br2.ReadString();
                                                    fileline1 = br4.ReadString();
                                                    fileTerm = fileline.Substring(0, fileline.IndexOf('¥'));
                                                }
                                                //get the first line of the queue
                                                bool haveNext = it.MoveNext();
                                                if (haveNext)
                                                {
                                                    listline = it.Current.Value.TermDic();
                                                    listline1 = it.Current.Value.TermPost();
                                                    listTerm = it.Current.Key;
                                                    haveNext = it.MoveNext();
                                                }

                                                while (haveNext && br2.BaseStream.Position != br2.BaseStream.Length)
                                                {
                                                    int n = String.Compare(fileTerm, listTerm);
                                                    // -1 left smaller than right. 1 right smaller than left. 0 equals
                                                    if (n == -1)
                                                    {
                                                        //write the fileline into the new file
                                                        bw1.Write(fileline);
                                                        bw3.Write(fileline1);
                                                        //read a new line from the file
                                                        fileline = br2.ReadString();
                                                        fileline1 = br4.ReadString();
                                                        fileTerm = fileline.Substring(0, fileline.IndexOf('¥'));
                                                    }
                                                    else if (n == 1)
                                                    {
                                                        //write the listline into the new file
                                                        bw1.Write(listline);
                                                        bw3.Write(listTerm + "¥" + listline1);
                                                        //read a new line from the list
                                                        listline = it.Current.Value.TermDic();
                                                        listline1 = it.Current.Value.TermPost();
                                                        haveNext = it.MoveNext();
                                                        listTerm = listline.Substring(0, listline.IndexOf('¥'));
                                                    }
                                                    else
                                                    {
                                                        //merge the two lines
                                                        newline = meregeLines(fileline, listline);
                                                        newline2 = fileline1 + listline1;
                                                        //write the merge string into the new file
                                                        bw1.Write(newline);
                                                        bw3.Write(newline2);
                                                        //read a new line from the file
                                                        fileline = br2.ReadString();
                                                        fileline1 = br4.ReadString();
                                                        fileTerm = fileline.Substring(0, fileline.IndexOf('¥'));
                                                        //read a new line from the list
                                                        listline = it.Current.Value.TermDic();
                                                        listline1 = it.Current.Value.TermPost();
                                                        haveNext = it.MoveNext();
                                                        listTerm = listline.Substring(0, listline.IndexOf('¥'));
                                                    }
                                                }

                                                //case the two lines finished
                                                if (!haveNext && br2.BaseStream.Position == br2.BaseStream.Length)
                                                {
                                                    int n = String.Compare(fileTerm, listTerm);
                                                    if (n == -1)
                                                    {
                                                        //write the last lines
                                                        bw1.Write(fileline);
                                                        bw3.Write(fileline1);
                                                        bw1.Write(listline);
                                                        bw3.Write(listTerm + "¥" + listline1);
                                                    }
                                                    else if (n == 1)
                                                    {
                                                        bw1.Write(listline);
                                                        bw3.Write(listTerm + "¥" + listline1);
                                                        bw1.Write(fileline);
                                                        bw3.Write(fileline1);
                                                    }
                                                    else
                                                    {
                                                        newline = meregeLines(fileline, listline);
                                                        newline2 = fileline1 + listline1;
                                                        bw1.Write(newline);
                                                        bw3.Write(newline2);
                                                    }
                                                }
                                                else
                                                {
                                                    if (!haveNext)
                                                    {
                                                        //case the list finished
                                                        while (listTerm != "" && fileTerm != "")
                                                        {
                                                            int n = String.Compare(fileTerm, listTerm);
                                                            if (n == -1)
                                                            {
                                                                bw1.Write(fileline);
                                                                bw3.Write(fileline1);
                                                                //check if the file finished
                                                                if (br2.BaseStream.Position != br2.BaseStream.Length)
                                                                {
                                                                    fileline = br2.ReadString();
                                                                    fileline1 = br4.ReadString();
                                                                    fileTerm = fileline.Substring(0, fileline.IndexOf('¥'));
                                                                }
                                                                else
                                                                {
                                                                    //flag that the file finished
                                                                    fileTerm = "";
                                                                }
                                                            }
                                                            else if (n == 1)
                                                            {
                                                                bw1.Write(listline);
                                                                bw3.Write(listTerm + "¥" + listline1);
                                                                //flag that the list finished
                                                                listTerm = "";
                                                            }
                                                            else
                                                            {
                                                                newline = meregeLines(fileline, listline);
                                                                newline2 = fileline1 + listline1;
                                                                bw1.Write(newline);
                                                                bw3.Write(newline2);
                                                                listTerm = "";
                                                                if (br2.BaseStream.Position == br2.BaseStream.Length)
                                                                {
                                                                    //flag that the file finished
                                                                    fileTerm = "";
                                                                }
                                                                else
                                                                {
                                                                    fileline = br2.ReadString();
                                                                    fileline1 = br4.ReadString();
                                                                    fileTerm = fileline.Substring(0, fileline.IndexOf('¥'));
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //case the file finished
                                                        while (listTerm != "" && fileTerm != "")
                                                        {
                                                            int n = String.Compare(fileTerm, listTerm);
                                                            if (n == -1)
                                                            {
                                                                bw1.Write(fileline);
                                                                bw3.Write(fileline1);
                                                                //flag that the file finished
                                                                fileTerm = "";
                                                            }
                                                            else if (n == 1)
                                                            {
                                                                bw1.Write(listline);
                                                                bw3.Write(listTerm + "¥" + listline1);
                                                                if (haveNext)
                                                                {
                                                                    listline = it.Current.Value.TermDic();
                                                                    listline1 = it.Current.Value.TermPost();
                                                                    haveNext = it.MoveNext();
                                                                    listTerm = listline.Substring(0, listline.IndexOf('¥'));
                                                                }
                                                                else
                                                                {
                                                                    //flag that the list finished
                                                                    listTerm = "";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                newline = meregeLines(fileline, listline);
                                                                newline2 = fileline1 + listline1;
                                                                bw1.Write(newline);
                                                                bw3.Write(newline2);
                                                                fileTerm = "";
                                                                if (haveNext)
                                                                {
                                                                    listline = it.Current.Value.TermDic();
                                                                    listline1 = it.Current.Value.TermPost();
                                                                    haveNext = it.MoveNext();
                                                                    listTerm = listline.Substring(0, listline.IndexOf('¥'));
                                                                }
                                                                else
                                                                {
                                                                    //flag that the list finished
                                                                    listTerm = "";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //write the lines that reminds from the dictionary or the list
                                                    if (!(listTerm == "" && fileTerm == ""))
                                                    {
                                                        if (listTerm == "")
                                                        {
                                                            bw1.Write(fileline);
                                                            bw3.Write(fileline1);
                                                            while (br2.BaseStream.Position != br2.BaseStream.Length)
                                                            {
                                                                fileline = br2.ReadString();
                                                                fileline1 = br4.ReadString();
                                                                bw1.Write(fileline);
                                                                bw3.Write(fileline1);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bw1.Write(listline);
                                                            bw3.Write(listTerm + "¥" + listline1);
                                                            while (haveNext)
                                                            {
                                                                listline = it.Current.Value.TermDic();
                                                                listline1 = it.Current.Value.TermPost();
                                                                haveNext = it.MoveNext();
                                                                bw1.Write(listline);
                                                                bw3.Write(listTerm + "¥" + listline1);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            //delete the previos file 
            File.Delete(pathToAddD);
            File.Delete(pathToAddP);
            //name the temp file to his name
            File.Move(tempPathD, pathToAddD);
            File.Move(tempPathP, pathToAddP);
        }

        /// <summary>
        /// functiion to create the update line after the merge
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private string meregeLines(string line1, string line2)
        {
            string newline = "";
            char[] seperators = { '¥', '€' };
            string[] sepLine1 = line1.Split(seperators);
            string[] sepLine2 = line2.Split(seperators);
            //get num of each term toatl appear
            int timet1, timet2;
            int.TryParse(sepLine1[1], out timet1);
            int.TryParse(sepLine2[1], out timet2);
            int sumOfotalTerms = timet1 + timet2;
            //get num of docs in each term 
            int timed1, timed2;
            int.TryParse(sepLine1[2], out timed1);
            int.TryParse(sepLine2[2], out timed2);
            int sumOfotalDocs = timed1 + timed2;

            newline = sepLine1[0] + '¥' + sumOfotalTerms + '€' + sumOfotalDocs;
            return newline;
        }

        /// <summary>
        /// create dictionary of all terms
        /// </summary>
        /// <param name="stem"> is to get the stem files</param>
        public void getTermsToDictionary()
        {
            dic = new Dictionary<string, string>();
            string pathToOpen;
            if (isStem)
                pathToOpen = pathTermDicS;
            else
                pathToOpen = pathTermDic;
            for (int i = 0; i < 27; i++)
            {
                string path2 = pathToOpen + getNameOfFile(i) + ".txt";
                getDictionary(path2);
            }
        }

        /// <summary>
        /// get the terms of all the files
        /// </summary>
        /// <param name="path"> path the path to the file</param>
        private void getDictionary(string path)
        {
            try
            {
                using (Stream s = new FileStream(path, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(s))
                    {
                        while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            numOfUniqeWords++;
                            string line = br.ReadString();
                            char[] sep = new char[] { '¥', '€' };
                            string[] T = line.Split(sep);
                            dic.Add(T[0], T[1]);
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
        /// get the string og the dictionery for display
        /// </summary>
        /// <returns>the string to display</returns>
        public string getDisplayR()
        {
            StringBuilder display = new StringBuilder();
            foreach (KeyValuePair<string, string> term in dic)
            {
                display.Append(term.Key + ": " + term.Value + "\n");
            }

            return display.ToString();
        }
    }
}