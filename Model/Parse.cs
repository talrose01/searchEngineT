using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


namespace SearchEngineP.Model
{
    public class Parse
    {
        public static Dictionary<string, string> monthDic; //dictionary of month in numbers and in words
        private Dictionary<string, Term> termsOfDocDic; // dictionary of all terms in document
        private Dictionary<string, Term> termsOfFile; //dictionary of all terms in some files
        //private char[] charsToTrim = { '.', ',', '/', ';', ':', '~', '"', '|', '[', ']', '(', ')', '{', '}', '?', '!', '@', '#', '^', '&', '*', '`', '\\', '-', '_', '+', '=' };
        char[] charsToTSplit = { ';', '(', ')', '[', ']', '{', '}' };
        private Doc doc; //a doc object
        public static bool isStem; // hold if is stemming
        private string docName; //hold document name
        private string[] termsBySpaces; //hold the text split by spaces
        static Regex range = new Regex(@"(\w+-(-)?\w+-(-)?\w+)|((\w+|(\d+(,\d+)*((\.\d+)|(\s\d+(,\d+)*\/\d+(,\d+)*)|(\/\d+(,\d+)*))?((\s)+(million|billion|trillion))?))-(-)?((\d+(,\d+)*((\.\d+)|(\s\d+(,\d+)*\/\d+(,\d+)*)|(\/\d+(,\d+)*))?((\s)+(million|billion|trillion))?)|\w+))", RegexOptions.IgnoreCase);

        /// <summary>
        /// start a new parse activity
        /// </summary>
        /// <param name="docText">the text of the doc</param>
        /// <param name="termsOfFile">the terms we should parse</param>
        /// <param name="docInfo">the information of docs</param>
        public void startParsing(ref string docText, ref Dictionary<string, Term> termsOfFile, ref HashSet<string> docInfo)
        {
            termsOfDocDic = new Dictionary<string, Term>();
            this.termsOfFile = termsOfFile;
            convertToTerms(ref docText);
            lock (ReadFile.top5)
            {
                ReadFile.totalNumOfWords = ReadFile.totalNumOfWords + doc.numOfTerms;
            }
            docInfo.Add(doc.DocDetails());
        }

        /// <summary>
        /// update the information of the doc of the file 
        /// </summary>
        /// <param name="docName">docName</param>
        /// <param name="docDate">docDate</param>
        /// <param name="docLanguage">docLanguage</param>
        public void insertDocInfo(ref string docName, ref string docDate, ref string docLanguage)
        {
            this.docName = docName;
            doc = new Doc(docName);
            if (docDate != "")
                doc.Date = docDate;
            if (docLanguage != "")
                doc.SourceLanguage = docLanguage;
        }

        /// <summary>
        ///find all the terms in the doc
        /// </summary>
        /// <param name="TextOfDoc">the text we should parse</param>
        private void convertToTerms(ref string TextOfDoc)
        {
            //clean the text from spaces
            string Cleantext = TextOfDoc.Replace("\n", " ").Replace("\r", " ").ToLower();
            termsBySpaces = Cleantext.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < termsBySpaces.Length; i++)
            {
                termsBySpaces[i] = cleanGarbge(termsBySpaces[i]);
                if (i + 1 < termsBySpaces.Length)
                {
                    string term2 = cleanGarbge(termsBySpaces[i + 1]);
                    if (termsBySpaces[i] != "")
                    {
                        lock (ReadFile.timesOfN)
                        {
                            if (ReadFile.timesOfN.ContainsKey(termsBySpaces[i]))
                            {
                                if (ReadFile.timesOfN[termsBySpaces[i]].ContainsKey(term2))
                                {
                                    ReadFile.timesOfN[termsBySpaces[i]][term2]++;
                                }
                                else
                                {
                                    ReadFile.timesOfN[termsBySpaces[i]].Add(term2, 1);
                                }
                            }
                            else
                            {
                                Dictionary<string, int> dic = new Dictionary<string, int>();
                                dic.Add(term2, 1);
                                ReadFile.timesOfN.Add(termsBySpaces[i], dic);
                            }
                        }
                    }

                    if (!checkNumber(i))
                    {
                        if (!checkPercent(i))
                        {
                            if (!checkPrice(i))
                            {
                                if (!checkDate(i))
                                {
                                    checkRange(i);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// check and create a number term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private bool checkNumber(int index)
        {
            string value = termsBySpaces[index];
            string s = convertToNumber(value);
            string termToAdd = "";
            double num;
            if (double.TryParse(s, out num))
            {
                if (num >= 1000000) //check if it million number
                {
                    num = num / 1000000;
                    termToAdd = "" + num + "M";
                    //check if it is price
                    if (index + 1 < termsBySpaces.Length)
                    {
                        string value2 = cleanGarbge(termsBySpaces[index + 1]);
                        if (value2 == "dollars") // check if it a dollar term
                        {
                            string termToAdd2 = termToAdd + " " + value2;
                            addTermDic(termToAdd2);
                        }
                        else
                        {
                            if (index + 2 < termsBySpaces.Length)
                            {
                                string value3 = cleanGarbge(termsBySpaces[index + 2]);
                                if (value2 == "u.s." && value3 == "dollars") //check if it us dollar term
                                {
                                    string termToAdd2 = termToAdd + " dollars";
                                    addTermDic(termToAdd2);
                                }
                                else
                                {
                                    checkIfLengthOrWeight(num, value2);
                                }
                            }
                            else
                            {
                                checkIfLengthOrWeight(num, value2);
                            }
                        }
                    }
                }
                else
                {
                    if (index + 1 < termsBySpaces.Length)
                    {
                        string value2 = cleanGarbge(termsBySpaces[index + 1]);
                        if (value2 == "million") // check if it number with million
                        {
                            termToAdd = "" + num + "M";
                            num = num * 1000000;
                            //check if it is a price
                            if (index + 3 < termsBySpaces.Length)
                            {
                                string value4 = cleanGarbge(termsBySpaces[index + 3]);
                                if (termsBySpaces[index + 2] == "u.s." && value4 == "dollars")
                                {
                                    string termToAdd2 = termToAdd + " dollars";
                                    addTermDic(termToAdd2);
                                }
                                else
                                {
                                    checkIfLengthOrWeight(num, value2);
                                }
                            }
                            else
                            {
                                checkIfLengthOrWeight(num, value2);
                            }
                        }
                        else
                        {
                            if (value2 == "billion") // check if it number with billion
                            {
                                termToAdd = "" + num * 1000 + "M";
                                num = num * 1000000000;
                                //check if it is a price
                                if (index + 3 < termsBySpaces.Length)
                                {
                                    string value4 = cleanGarbge(termsBySpaces[index + 3]);
                                    if (termsBySpaces[index + 2] == "u.s." && value4 == "dollars")
                                    {
                                        string termToAdd2 = termToAdd + " dollars";
                                        addTermDic(termToAdd2);
                                    }
                                    else
                                    {
                                        checkIfLengthOrWeight(num, value2);
                                    }

                                }
                                else
                                {
                                    checkIfLengthOrWeight(num, value2);
                                }
                            }
                            else
                            {
                                if (value2 == "trillion") // check if it number with trillion
                                {
                                    termToAdd = "" + num * 1000000 + "M";
                                    num = num * 1000000000000;
                                    //check if it is a price
                                    if (index + 3 < termsBySpaces.Length)
                                    {
                                        string value4 = cleanGarbge(termsBySpaces[index + 3]);
                                        if (termsBySpaces[index + 2] == "u.s." && value4 == "dollars")
                                        {
                                            string termToAdd2 = termToAdd + " dollars";
                                            addTermDic(termToAdd2);
                                        }
                                        else
                                        {
                                            checkIfLengthOrWeight(num, value2);
                                        }
                                    }
                                    else
                                    {
                                        checkIfLengthOrWeight(num, value2);
                                    }
                                }
                                else
                                {
                                    double sheverA, sheverB;
                                    string[] shever = value2.Split('/'); //check if it fraction
                                    if (shever.Length == 2 && double.TryParse(shever[0], out sheverA) && double.TryParse(shever[1], out sheverB))
                                    {
                                        termToAdd = value;
                                        string termToAdd2 = value + " " + value2;
                                        addTermDic(termToAdd2);
                                        //check if it is price
                                        if (index + 2 < termsBySpaces.Length)
                                        {
                                            string value3 = cleanGarbge(termsBySpaces[index + 2]);
                                            if (value3 == "dollars") // check if it fraction with dollar
                                            {
                                                string termToAdd3 = termToAdd2 + " " + value3;
                                                addTermDic(termToAdd3);
                                            }
                                            else
                                            {
                                                if (index + 3 < termsBySpaces.Length)
                                                {
                                                    string value4 = cleanGarbge(termsBySpaces[index + 3]);
                                                    if (value3 == "u.s." && value4 == "dollars") // check if it fraction with us dollar
                                                    {
                                                        string termToAdd3 = termToAdd2 + " dollars";
                                                        addTermDic(termToAdd3);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        termToAdd = value;
                                        //check if it is percent
                                        if (value2 == "percent" || value2 == "percentage")
                                        {
                                            string termToAdd2 = termToAdd + "%";
                                            addTermDic(termToAdd2);
                                        }
                                        else
                                        {
                                            //check if it is price
                                            if (value2 == "dollars")
                                            {
                                                string termToAdd2 = termToAdd + " " + value2;
                                                addTermDic(termToAdd2);
                                            }
                                            else
                                                if (!checkIfLengthOrWeight(num, value2))
                                            {
                                                if (num > 0 && num < 32)
                                                    checkDate(index);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                addTermDic(termToAdd);
                return true;
            }
            else
            {
                if (s.Contains('/'))
                {
                    string[] nums = s.Split('/');
                    //check if it is price
                    if (nums.Length == 2)
                    {
                        double num2;
                        double num3;
                        if (double.TryParse(nums[0], out num2) && double.TryParse(nums[1], out num3))
                        {
                            termToAdd = s;
                            addTermDic(termToAdd);
                            return true;
                        }
                    }
                    else
                    {
                        foreach (string n1 in nums)
                        {
                            checkNumberString(n1);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// check and create a percent term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private bool checkPercent(int index)
        {
            if (termsBySpaces[index].Length > 1 && termsBySpaces[index][termsBySpaces[index].Length - 1] == '%')
            {
                addTermDic(termsBySpaces[index]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// check and create a price term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private bool checkPrice(int index)
        {
            string termToAdd = "";
            if (termsBySpaces[index][0] == '$')
            {
                string number = termsBySpaces[index].Substring(1);
                string s = convertToNumber(number);
                double num;
                if (double.TryParse(s, out num))
                {
                    if (num >= 1000000)
                    {
                        termToAdd = "" + num / 1000000 + "M dollars";
                        addTermDic(termToAdd);
                        return true;

                    }
                    else
                    {
                        if (index + 1 < termsBySpaces.Length)
                        {
                            string value2 = cleanGarbge(termsBySpaces[index + 1]);
                            if (value2 == "million")
                            {
                                termToAdd = "" + num + "M dollars";
                                addTermDic(termToAdd);
                                return true;
                            }
                            else
                            {
                                if (value2 == "billion")
                                {
                                    termToAdd = "" + num * 1000 + "M dollars";
                                    addTermDic(termToAdd);
                                    return true;
                                }
                                else
                                {
                                    if (value2 == "trillion")
                                    {
                                        termToAdd = "" + num * 1000000 + "M dollars";
                                        addTermDic(termToAdd);
                                        return true;
                                    }
                                    else
                                    {
                                        if (value2.Contains('/'))
                                        {
                                            double sheverA, sheverB;
                                            string[] shever = value2.Split('/');
                                            if (shever.Length == 2 && double.TryParse(shever[0], out sheverA) && double.TryParse(shever[1], out sheverB))
                                            {
                                                termToAdd = s + " " + value2 + " dollars";
                                                addTermDic(termToAdd);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            termToAdd = s + " dollars";
                                            addTermDic(termToAdd);
                                            return true;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //case it is m/bn case
                string number = termsBySpaces[index];
                double number1;
                if (number.Length > 2 && index + 1 < termsBySpaces.Length)
                {
                    string twoLastWord = number.Substring(number.Length - 2, 2);
                    string value2 = cleanGarbge(termsBySpaces[index + 1]);
                    if (value2 == "dollars")
                    {
                        if (twoLastWord == "bn")
                        {
                            number = number.Substring(0, number.Length - 2);
                            string s = convertToNumber(number);
                            double.TryParse(s, out number1);
                            number1 = number1 * 1000;
                            termToAdd = number1 + "M dollars";
                            addTermDic(termToAdd);
                            return true;
                        }
                        else
                        {
                            if (number.Length > 1)
                            {
                                //case it can be m number
                                string oneLastWord = number.Substring(number.Length - 1, 1);
                                if (oneLastWord == "m")
                                {
                                    number = number.Substring(0, number.Length - 1);
                                    string s = convertToNumber(number);
                                    double.TryParse(s, out number1);
                                    termToAdd = number1 + "M Dollars";
                                    addTermDic(termToAdd);
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (number.Length > 1)
                    {
                        //case it can be m number
                        string oneLastWord = number.Substring(number.Length - 1, 1);
                        if (oneLastWord == "m")
                        {
                            number = number.Substring(0, number.Length - 1);
                            string s = convertToNumber(number);
                            double.TryParse(s, out number1);
                            termToAdd = number1 + "M Dollars";
                            addTermDic(termToAdd);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// check and create a date term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private bool checkDate(int index)
        {
            string termToAdd = "";
            string day = "";
            string month = "";
            string year = "";
            int intYear;
            int dayDate;

            //check if the word is term
            string value = termsBySpaces[index];
            //formats :DD month yyyy, DD month yy, DD month
            if (int.TryParse(value, out dayDate))
            {
                if (dayDate > 0 && dayDate < 32)
                {
                    if (dayDate > 0 && dayDate < 10)
                    {
                        day = "0" + dayDate;
                    }
                    else if (dayDate > 9 && dayDate < 32)
                    {
                        day = "" + dayDate;

                    }
                    else
                        return false;
                    if (index + 1 < termsBySpaces.Length)
                    {
                        if (isMonth(termsBySpaces[index + 1]))
                        {
                            month = monthDic[termsBySpaces[index + 1]];

                            if (index + 2 < termsBySpaces.Length)
                            {
                                string yearTocheck = cleanGarbge(termsBySpaces[index + 2]);
                                if (int.TryParse(yearTocheck, out intYear))
                                {
                                    year = convertYear(intYear);
                                    termToAdd = year + "-" + month + "-" + day;
                                    addTermDic(termToAdd);
                                    return true;

                                }
                                else
                                {
                                    termToAdd = month + "-" + day;
                                    addTermDic(termToAdd);
                                    return true;
                                }
                            }

                        }
                        else
                            return false;
                    }
                }
            }
            else
            {
                //check if it is th format
                if (value.Length > 2)
                {
                    //formats :DDth month yyyy
                    string twoLastWords = value.Substring(termsBySpaces[index].Length - 2);
                    if (twoLastWords == "th")
                    {
                        if (int.TryParse(value.Substring(0, termsBySpaces[index].Length - 2), out dayDate))
                        {
                            if (dayDate > 0 && dayDate < 10)
                            {
                                day = "0" + dayDate;
                            }
                            else if (dayDate > 9 && dayDate < 32)
                            {
                                day = "" + dayDate;
                            }
                            else
                                return false;
                            if (index + 1 < termsBySpaces.Length)
                            {
                                if (isMonth(termsBySpaces[index + 1]))
                                {
                                    month = monthDic[termsBySpaces[index + 1]];
                                    if (index + 2 < termsBySpaces.Length)
                                    {
                                        if (int.TryParse(termsBySpaces[index + 2], out intYear))
                                        {
                                            year = convertYear(intYear);
                                            termToAdd = year + "-" + month + "-" + day;
                                            addTermDic(termToAdd);
                                            return true;
                                        }
                                        else
                                        {
                                            termToAdd = month + "-" + day;
                                            addTermDic(termToAdd);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //formats :month DD, yyyy  month DD   month yyyy
                        if (isMonth(value))
                        {
                            month = monthDic[value];
                            if (index + 1 < termsBySpaces.Length)
                            {
                                if (int.TryParse(cleanGarbge(termsBySpaces[index + 1]), out dayDate))
                                {
                                    if (dayDate > 0 && dayDate < 10)
                                    {
                                        day = "0" + dayDate;
                                    }
                                    else if (dayDate > 9 && dayDate < 32)
                                    {
                                        day = "" + dayDate;
                                    }
                                    else if (dayDate > 999 & dayDate < 3000)
                                    {
                                        year = "" + dayDate;
                                        termToAdd = year + "-" + month;
                                    }

                                    //formats :month DD, yyyy  month DD 
                                    if (dayDate > 0 && dayDate < 32)
                                    {
                                        if (index + 2 < termsBySpaces.Length)
                                        {
                                            if (int.TryParse(cleanGarbge(termsBySpaces[index + 2]), out intYear))
                                            {
                                                year = convertYear(intYear);
                                                if (year != "")
                                                    termToAdd = year + "-" + month + "-" + day;
                                                else
                                                    termToAdd = month + "-" + day;
                                                addTermDic(termToAdd);
                                                return true;
                                            }
                                            else
                                            {
                                                termToAdd = month + "-" + day;
                                                addTermDic(termToAdd);
                                                return true;
                                            }

                                        }
                                    }
                                    //month yyyy
                                    else
                                    {
                                        if (int.TryParse(termsBySpaces[index + 1], out intYear))
                                        {
                                            year = convertYear(intYear);
                                            termToAdd = year + "-" + month;
                                            addTermDic(termToAdd);
                                            return true;
                                        }

                                    }


                                }
                            }
                        }

                    }
                }

            }
            return false;

        }

        /// <summary>
        /// check and create a range term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private void checkRange(int index)
        {
            string[] words = termsBySpaces[index].Split('-');
            MatchCollection termRange = range.Matches(termsBySpaces[index]);
            if (termRange.Count > 0)
            {
                foreach (Match v in termRange)
                {
                    addTermDic(v.ToString());
                }
            }
            else
            {
                string value = termsBySpaces[index];
                //range: between _ and  _
                if (value == "between")
                {
                    if (index + 3 < termsBySpaces.Length)
                    {
                        if (termsBySpaces[index + 2] == "and")
                        {
                            string value4 = cleanGarbge(termsBySpaces[index + 3]);
                            string termToAdd = termsBySpaces[index] + " " + termsBySpaces[index + 1] + " " + termsBySpaces[index + 2] + " " + value4;
                            addTermDic(termToAdd);
                        }
                    }
                }
            }
            foreach (string w in words)
                checkWord(w);
        }

        /// <summary>
        /// check if the term is length term or weight term
        /// </summary>
        /// <param name="num">the number</param>
        /// <param name="measure">t</param>
        private bool checkIfLengthOrWeight(double num, string measure)
        {
            //check if it length
            string termToAdd2 = ParseNumberMeter(num, measure);
            if (termToAdd2 != "")
            {
                addTermDic(termToAdd2);
                return true;
            }

            else
            {
                termToAdd2 = ParseNumberGram(num, measure);
                if (termToAdd2 != "")
                {
                    addTermDic(termToAdd2);
                    return true;
                }

            }
            return false;
        }

        private bool isWord(string cut)
        {
            if ((cut[0] >= 'a' && cut[0] <= 'z') || (cut[0] >= '0' && cut[0] <= '9'))
            {
                if (cut.Length > 1)
                {
                    if ((cut[cut.Length - 1] >= 'a' && cut[cut.Length - 1] <= 'z') || (cut[cut.Length - 1] >= '0' && cut[cut.Length - 1] <= '9'))
                        return true;
                    else
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// check and create a word term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        /// <summary>
        /// check and create a word term
        /// </summary>
        /// <param name="index">index in the words array</param>
        private void checkWord(string value)
        {
            string clean = "";
            string toAdd = "";
            string[] words = value.Split(charsToTSplit, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
            {

                if (w != "")
                {
                    if (!ReadFile.stopWordsDic.Contains(w))
                    {
                        clean = cleanGarbge(w);

                        if (!addNum(clean) && clean != "")
                        {
                            //case nor number
                            if (isStem)
                            {
                                lock (ReadFile.stemmer)
                                {
                                    toAdd = ReadFile.stemmer.stemTerm(clean);
                                }

                            }
                            else
                                toAdd = clean;
                            addTermDic(toAdd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check and create a number term
        /// </summary>
        /// <param name="index">word in the words array</param>
        private bool addNum(string value)
        {
            string s = convertToNumber(value);
            double num;
            if (double.TryParse(s, out num))
            {
                string termToAdd = "";
                if (num >= 1000000)
                {
                    num = num / 1000000;
                    termToAdd = "" + num + "M";
                }
                else
                {
                    termToAdd = value;
                }
                addTermDic(termToAdd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ass the term in to the dictionary
        /// </summary>
        /// <param name="termToAdd">the term that we find</param>
        private void addTermDic(string termToAdd)
        {
            Term term;
            if (termToAdd != "")
            {
                //add to doc dic
                doc.addTermToDocDic(termToAdd);
                //increase num of words of the doc
                if (!termsOfDocDic.ContainsKey(termToAdd))
                {
                    term = new Term(termToAdd);
                    termsOfDocDic.Add(termToAdd, term);
                    if (!termsOfFile.ContainsKey(termToAdd))
                    {
                        //add to the term dic
                        termsOfFile.Add(termToAdd, termsOfDocDic[termToAdd]);
                    }
                }// should update the term anyway
                termsOfFile[termToAdd].updateDocShows(docName, doc.SourceLanguage);
            }
        }

        //******************function to help******************************

        /// <summary>
        /// remove tabs from edge 
        /// </summary>
        /// <param name="word">the string to clean</param>
        private string cleanGarbge(string word)
        {
            string cut = word;

            while (!((cut[0] >= 'a' && cut[0] <= 'z') || (cut[0] >= '0' && cut[0] <= '9')))
            {
                if (cut.Length > 1)
                    cut = cut.Substring(1);
                else
                {
                    cut = "";
                    return cut;
                }
            }
            while (!((cut[cut.Length - 1] >= 'a' && cut[cut.Length - 1] <= 'z') || (cut[cut.Length - 1] >= '0' && cut[cut.Length - 1] <= '9')))
            {
                char c = cut[cut.Length - 1];
                cut = cut.Substring(0, cut.Length - 1);
                if (cut == "")
                    return cut;

            }


            return cut;
        }

        /// <summary>
        /// check if a stirng is month
        /// </summary>
        /// <param name="month"</param>
        /// <returns>true if its month</returns>
        private bool isMonth(string month)
        {
            if (monthDic.ContainsKey(month))
                return true;
            return false;
        }

        private string convertYear(int year)
        {
            string yearReturn = "";

            if (year < 100)
            {
                if (year > 20)
                {
                    yearReturn = "19" + year;
                }
                else
                {
                    yearReturn = "20" + year;
                }
            }
            else
            {
                return year.ToString();
            }
            return yearReturn;
        }

        /// <summary>
        /// convert num with "," to a number without
        /// </summary>
        /// <param name="s"></param>
        /// <returns>the number without ,</returns>
        private string convertToNumber(string s)
        {
            string[] millionNumber = s.Split(',');
            string ans = "";
            for (int i = 0; i < millionNumber.Length; i++)
            {
                ans = ans + millionNumber[i];
            }
            return ans;
        }

        /// <summary>
        /// to create aterm of length by its rule
        /// </summary>
        /// <param name="num">the number</param>
        /// <param name="length">the measure</param>
        /// <returns></returns>
        private string ParseNumberMeter(double num, string length)
        {
            string returnValue = "";
            if (length == "m" || length == "meter" || length == "meters")
                returnValue = "" + "m";
            //case it is a billion
            else if (length == "km" || length == "kilometer" || length == "kilometers")
                returnValue = num * 1000 + "m";
            else if (length == "cm" || length == "centimeter" || length == "centimeters")
                returnValue = num / 100 + "m";
            else if (length == "mm" || length == "millimeter" || length == "millimeters")
                returnValue = num / 1000 + "m";
            else if (length == "nm" || length == "nanometer" || length == "nanometers")
                returnValue = num / 1000000000 + "m";
            //now the double is in meters
            return returnValue;
        }

        /// <summary>
        /// to create aterm of whight by its rule
        /// </summary>
        /// <param name="doubleNumber">the number</param>
        /// <param name="numberKindPart">the measure</param>
        /// <returns></returns>
        private string ParseNumberGram(double doubleNumber, string numberKindPart)
        {
            string termToAdd = "";
            if (numberKindPart == "g" || numberKindPart == "gram" || numberKindPart == "grams")
                termToAdd = doubleNumber.ToString() + "g";
            //case it is a billion
            else if (numberKindPart == "kg" || numberKindPart == "kilogram" || numberKindPart == "kilograms")
                termToAdd = doubleNumber * 1000 + "g";
            else if (numberKindPart == "mg" || numberKindPart == "milligram" || numberKindPart == "milligrams")
                termToAdd = doubleNumber / 1000 + "g";
            else if (numberKindPart == "ng" || numberKindPart == "nanogram" || numberKindPart == "nanograms")
                termToAdd = doubleNumber / 1000000000 + "g";
            return termToAdd;
        }

        /// <summary>
        /// save a number with ,
        /// </summary>
        /// <param name="doubleNumber"></param>
        /// <param name="kind">measure</param>
        /// <returns>the term we should save</returns>
        private string ParseNumber(double doubleNumber, string kind)
        {
            string termToAdd = "";
            if (doubleNumber.ToString().Length >= 7)
            {
                doubleNumber = doubleNumber / 1000000;
                termToAdd = doubleNumber.ToString() + "M";
            }
            else
            {
                if (doubleNumber < 999)
                    termToAdd = doubleNumber.ToString();
                else if (doubleNumber > 999 && doubleNumber < 10000)
                    termToAdd = doubleNumber.ToString()[0] + "," + doubleNumber.ToString().Substring(1);
                else if (doubleNumber > 9999 && doubleNumber < 100000)
                    termToAdd = doubleNumber.ToString().Substring(0, 2) + "," + doubleNumber.ToString().Substring(2);
                else if (doubleNumber > 99999 && doubleNumber < 1000000)
                    termToAdd = doubleNumber.ToString().Substring(0, 3) + "," + doubleNumber.ToString().Substring(3);
            }
            termToAdd = termToAdd + " " + kind;
            return termToAdd;
        }

        /// <summary>
        /// get a number and change it to the format we will save it
        /// </summary>
        /// <param name="numberToConvert"></param>
        /// <returns></returns>
        private string convertNumberToMillion(string numberToConvert)
        {
            Double doubleNumber;
            string returnNumber = "";
            string termLowChar = numberToConvert.ToLower();
            string[] numberSplitSpaces = termLowChar.ToString().Split(' ');
            string[] millionNumber = numberSplitSpaces[0].Split(',');
            string num = convertToNumber(numberSplitSpaces[0]);
            Double.TryParse(num.ToString(), out doubleNumber);
            if (numberSplitSpaces.Length == 1 || (numberSplitSpaces[1] != "million" || numberSplitSpaces[1] != "billion" || numberSplitSpaces[1] != "trillion"))
            {
                // case number smaller than million
                if (millionNumber.Length < 3)
                    returnNumber = numberToConvert;
                // case number bigger than million
                else if (millionNumber.Length >= 3)
                {
                    if (numberSplitSpaces.Length == 2)
                    {
                        returnNumber = numberToConvert;
                    }
                    else
                    {
                        doubleNumber = doubleNumber / 1000000;
                        returnNumber = doubleNumber.ToString() + "M";
                    }
                }
            }
            else
            {
                //case it is a million
                if (numberSplitSpaces[1] == "million")
                    returnNumber = doubleNumber.ToString() + "M";
                //case it is a billion
                else if (numberSplitSpaces[1] == "billion")
                {
                    doubleNumber = doubleNumber * 1000;
                    returnNumber = doubleNumber.ToString() + "M";
                }
                //case it is a trillion
                else if (numberSplitSpaces[1] == "trillion")
                {
                    doubleNumber = doubleNumber * 1000000;
                    returnNumber = doubleNumber.ToString() + "M";
                }
            }
            return returnNumber;
        }

        /// <summary>
        /// check if a string is a number and add to the dic
        /// </summary>
        /// <param name="s"></param>
        private void checkNumberString(string s)
        {
            double num;
            if (double.TryParse(s, out num))
            {
                addTermDic(s);
            }
            else
            {
                checkWord(s);
            }
        }


        /// <summary>
        /// create dic of month names for help
        /// </summary>
        public static void createMonthDic()
        {
            monthDic = new Dictionary<string, string>();
            if (monthDic.Count == 0)
            {
                monthDic.Add("january", "01");
                monthDic.Add("february", "02");
                monthDic.Add("march", "03");
                monthDic.Add("april", "04");
                monthDic.Add("may", "05");
                monthDic.Add("june", "06");
                monthDic.Add("july", "07");
                monthDic.Add("august", "08");
                monthDic.Add("september", "09");
                monthDic.Add("october", "10");
                monthDic.Add("november", "11");
                monthDic.Add("december", "12");
                monthDic.Add("jan", "01");
                monthDic.Add("feb", "02");
                monthDic.Add("mar", "03");
                monthDic.Add("apr", "04");
                monthDic.Add("jun", "06");
                monthDic.Add("jul", "07");
                monthDic.Add("aug", "08");
                monthDic.Add("sep", "09");
                monthDic.Add("oct", "10");
                monthDic.Add("nov", "11");
                monthDic.Add("dec", "12");
            }
        }

    }
}
