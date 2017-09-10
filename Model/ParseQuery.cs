using SearchEngineP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngineP
{
    public class ParseQuery
    {
        //private char[] charsToTrim = { '.', ',', '/', ';', ':', '~', '"', '|', '[', ']', '(', ')', '{', '}', '?', '!', '@', '#', '^', '&', '*', '`', '\\', '-', '_', '+', '=' };
        char[] charsToTSplit = { ';', '(', ')', '[', ']', '{', '}' };
        public static Dictionary<string, string> monthDic; //dictionary of month in numbers and in words
        private string[] Cleantext;
        private Query p_query;
        static Regex range = new Regex(@"(\w+-(-)?\w+-(-)?\w+)|((\w+|(\d+(,\d+)*((\.\d+)|(\s\d+(,\d+)*\/\d+(,\d+)*)|(\/\d+(,\d+)*))?((\s)+(million|billion|trillion))?))-(-)?((\d+(,\d+)*((\.\d+)|(\s\d+(,\d+)*\/\d+(,\d+)*)|(\/\d+(,\d+)*))?((\s)+(million|billion|trillion))?)|\w+))", RegexOptions.IgnoreCase);
        public void ParseTheQuery(ref Query query)
        {
            p_query = query;
             string sq= query.QueryS.ToLower();
            //Searcher.Quries.Add(p_query, new Dictionary<string, TermOfQuery>());
            Cleantext = sq.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            query.Size = Cleantext.Length;
            for (int j = 0; j < Cleantext.Length; j++)
            {
                Cleantext[j] = cleanGarbge(Cleantext[j]);
                if (Cleantext[j] != "")
                {
                    if (!checkNumber(j))
                    {
                        if (!checkPercent(j))
                        {
                            if (!checkPrice(j))
                            {
                                if (!checkDate(j))
                                {
                                    checkRange(j);
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
            string s = convertToNumber(Cleantext[index]);
            string termToAdd = "";
            double num;
            if (double.TryParse(s, out num))
            {
                if (num >= 1000000) //check if it million number
                {
                    num = num / 1000000;
                    termToAdd = "" + num + "M";
                    Searcher.SearchWordOfQuery(termToAdd, p_query);
                    //check if it is price
                    if (index + 1 < Cleantext.Length)
                    {
                        string value2 = cleanGarbge(Cleantext[index + 1]);
                        if (value2 == "dollars") // check if it a dollar term
                        {
                            string termToAdd2 = termToAdd + " " + value2;
                            Searcher.SearchWordOfQuery(termToAdd2, p_query);
                        }
                        else
                        {
                            CheckUsDollars(index, value2, termToAdd, num);
                        }
                        return true;
                    }
                }
                else
                {
                    if (index + 1 < Cleantext.Length)
                    {
                        string value2 = cleanGarbge(Cleantext[index + 1]);
                        if (value2 == "million") // check if it number with million
                        {
                            termToAdd = "" + num + "M";
                            Searcher.SearchWordOfQuery(termToAdd, p_query);
                            num = num * 1000000;
                            //check if it is a price
                            if (index + 2 < Cleantext.Length)
                            {
                                string value3 = cleanGarbge(Cleantext[index + 2]);
                                CheckUsDollars(index + 1, value3, termToAdd, num);
                            }
                            return true;
                        }
                        else
                        {
                            if (value2 == "billion") // check if it number with billion
                            {
                                termToAdd = "" + num * 1000 + "M";
                                Searcher.SearchWordOfQuery(termToAdd, p_query);
                                num = num * 1000000000;
                                //check if it is a price
                                if (index + 2 < Cleantext.Length)
                                {
                                    string value3 = cleanGarbge(Cleantext[index + 2]);
                                    CheckUsDollars(index + 1, value3, termToAdd, num);
                                }
                                return true;
                            }
                            else
                            {
                                if (value2 == "trillion") // check if it number with trillion
                                {
                                    termToAdd = "" + num * 1000000 + "M";
                                    Searcher.SearchWordOfQuery(termToAdd, p_query);
                                    num = num * 1000000000000;
                                    //check if it is a price
                                    if (index + 2 < Cleantext.Length)
                                    {
                                        string value3 = cleanGarbge(Cleantext[index + 2]);
                                        CheckUsDollars(index + 1, value3, termToAdd, num);
                                    }
                                    return true;
                                }
                                else
                                {
                                    double sheverA, sheverB;
                                    string[] shever = value2.Split('/'); //check if it fraction
                                    if (shever.Length == 2 && double.TryParse(shever[0], out sheverA) && double.TryParse(shever[1], out sheverB))
                                    {
                                        termToAdd = Cleantext[index];
                                        Searcher.SearchWordOfQuery(termToAdd, p_query);
                                        string termToAdd2 = termToAdd + " " + value2;
                                        Searcher.SearchWordOfQuery(termToAdd2, p_query);
                                        //check if it is price
                                        if (index + 2 < Cleantext.Length)
                                        {
                                            string value3 = cleanGarbge(Cleantext[index + 2]);
                                            if (value3 == "dollars") // check if it fraction with dollar
                                            {
                                                string termToAdd3 = termToAdd2 + " " + value3;
                                                Searcher.SearchWordOfQuery(termToAdd3, p_query);
                                            }
                                            else
                                            {
                                                if (index + 3 < Cleantext.Length)
                                                {
                                                    string value4 = cleanGarbge(Cleantext[index + 3]);
                                                    if (value3 == "u.s." && value4 == "dollars") // check if it fraction with us dollar
                                                    {
                                                        string termToAdd3 = termToAdd2 + " dollars";
                                                        Searcher.SearchWordOfQuery(termToAdd3, p_query);
                                                    }
                                                }

                                            }
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        termToAdd = Cleantext[index];
                                        //check if it is percent
                                        if (value2 == "percent" || value2 == "percentage")
                                        {
                                            string termToAdd2 = termToAdd + "%";
                                            Searcher.SearchWordOfQuery(termToAdd2, p_query);
                                        }
                                        else
                                        {
                                            //check if it is price
                                            if (value2 == "dollars")
                                            {
                                                string termToAdd2 = termToAdd + " " + value2;
                                                Searcher.SearchWordOfQuery(termToAdd2, p_query);
                                            }
                                            else
                                                if (!checkIfLengthOrWeight(num, value2))
                                            {
                                                if (num > 0 && num < 32)
                                                    checkDate(index);
                                            }
                                        }
                                        return true;
                                    }

                                }
                            }
                        }
                    }
                }
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
                            Searcher.SearchWordOfQuery(termToAdd, p_query);

                        }
                        else
                        {
                            checkWord(cleanGarbge(nums[0]));
                            checkWord(cleanGarbge(nums[1]));
                        }
                        return true;
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
                return false;
            }

        }

        /// <summary>
        /// check and create a percent term
        /// </summary>
        /// <param name="index">index in the words array</param>
        /// <returns> true if find a term</returns>
        private bool checkPercent(int index)
        {
            if (Cleantext[index].Length > 1 && Cleantext[index][Cleantext[index].Length - 1] == '%')
            {
                Searcher.SearchWordOfQuery(Cleantext[index], p_query);
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
            if (Cleantext[index][0] == '$')
            {
                string number = Cleantext[index].Substring(1);
                string s = convertToNumber(number);
                double num;
                if (double.TryParse(s, out num))
                {
                    if (num >= 1000000)
                    {
                        termToAdd = "" + num / 1000000 + "M dollars";
                        Searcher.SearchWordOfQuery(termToAdd, p_query);
                        return true;

                    }
                    else
                    {
                        if (index + 1 < Cleantext.Length)
                        {
                            string value2 = cleanGarbge(Cleantext[index + 1]);
                            if (value2 == "million")
                            {
                                termToAdd = "" + num + "M dollars";
                                Searcher.SearchWordOfQuery(termToAdd, p_query);
                                return true;
                            }
                            else
                            {
                                if (value2 == "billion")
                                {
                                    termToAdd = "" + num * 1000 + "M dollars";
                                    Searcher.SearchWordOfQuery(termToAdd, p_query);
                                    return true;
                                }
                                else
                                {
                                    if (value2 == "trillion")
                                    {
                                        termToAdd = "" + num * 1000000 + "M dollars";
                                        Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                                                Searcher.SearchWordOfQuery(termToAdd, p_query);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            termToAdd = s + " dollars";
                                            Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                string number = Cleantext[index];
                double number1;
                if (number.Length > 2 && index + 1 < Cleantext.Length)
                {
                    string twoLastWord = number.Substring(number.Length - 2, 2);
                    string value2 = cleanGarbge(Cleantext[index + 1]);
                    if (value2 == "dollars")
                    {
                        if (twoLastWord == "bn")
                        {
                            number = number.Substring(0, number.Length - 2);
                            string s = convertToNumber(number);
                            double.TryParse(s, out number1);
                            number1 = number1 * 1000;
                            termToAdd = number1 + "M dollars";
                            Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                                    Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                            Searcher.SearchWordOfQuery(termToAdd, p_query);
                            return true;
                        }
                    }
                }
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
            string toAdd = "";
            string[] words = value.Split(charsToTSplit, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
            {
                if (w != "")
                {
                    if (!Searcher.stopWordsDic.Contains(w))
                    {
                        //case nor number
                        if (Searcher.isStem)
                        {
                            lock (Searcher.stemmer)
                            {
                                toAdd = Searcher.stemmer.stemTerm(w);
                            }

                        }
                        else
                            toAdd = w;
                        Searcher.SearchWordOfQuery(toAdd, p_query);
                    }
                }
            }
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
                Searcher.SearchWordOfQuery(s, p_query);
            }
            else
            {
                checkWord(s);
            }
        }

        private void CheckUsDollars(int index, string value2, string termToAdd, double num)
        {
            if (index + 2 < Cleantext.Length)
            {
                string value3 = cleanGarbge(Cleantext[index + 2]);
                if (value2 == "u.s." && value3 == "dollars") //check if it us dollar term
                {
                    string termToAdd2 = termToAdd + " dollars";
                    Searcher.SearchWordOfQuery(termToAdd2, p_query);
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
            string value = Cleantext[index];
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
                    if (index + 1 < Cleantext.Length)
                    {
                        if (isMonth(Cleantext[index + 1]))
                        {
                            month = monthDic[Cleantext[index + 1]];

                            if (index + 2 < Cleantext.Length)
                            {
                                string yearTocheck = cleanGarbge(Cleantext[index + 2]);
                                if (int.TryParse(yearTocheck, out intYear))
                                {
                                    year = convertYear(intYear);
                                    termToAdd = year + "-" + month + "-" + day;
                                    Searcher.SearchWordOfQuery(termToAdd, p_query);
                                    return true;

                                }
                                else
                                {
                                    termToAdd = month + "-" + day;
                                    Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                    string twoLastWords = value.Substring(Cleantext[index].Length - 2);
                    if (twoLastWords == "th")
                    {
                        if (int.TryParse(value.Substring(0, Cleantext[index].Length - 2), out dayDate))
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
                            if (index + 1 < Cleantext.Length)
                            {
                                if (isMonth(Cleantext[index + 1]))
                                {
                                    month = monthDic[Cleantext[index + 1]];
                                    if (index + 2 < Cleantext.Length)
                                    {
                                        if (int.TryParse(Cleantext[index + 2], out intYear))
                                        {
                                            year = convertYear(intYear);
                                            termToAdd = year + "-" + month + "-" + day;
                                            Searcher.SearchWordOfQuery(termToAdd, p_query);
                                            return true;
                                        }
                                        else
                                        {
                                            termToAdd = month + "-" + day;
                                            Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                            if (index + 1 < Cleantext.Length)
                            {
                                if (int.TryParse(cleanGarbge(Cleantext[index + 1]), out dayDate))
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
                                        if (index + 2 < Cleantext.Length)
                                        {
                                            if (int.TryParse(cleanGarbge(Cleantext[index + 2]), out intYear))
                                            {
                                                year = convertYear(intYear);
                                                if (year != "")
                                                    termToAdd = year + "-" + month + "-" + day;
                                                else
                                                    termToAdd = month + "-" + day;
                                                Searcher.SearchWordOfQuery(termToAdd, p_query);
                                                return true;
                                            }
                                            else
                                            {
                                                termToAdd = month + "-" + day;
                                                Searcher.SearchWordOfQuery(termToAdd, p_query);
                                                return true;
                                            }

                                        }
                                    }
                                    //month yyyy
                                    else
                                    {
                                        if (int.TryParse(Cleantext[index + 1], out intYear))
                                        {
                                            year = convertYear(intYear);
                                            termToAdd = year + "-" + month;
                                            Searcher.SearchWordOfQuery(termToAdd, p_query);
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
                Searcher.SearchWordOfQuery(termToAdd2, p_query);
                return true;
            }
            else
            {
                termToAdd2 = ParseNumberGram(num, measure);
                if (termToAdd2 != "")
                {
                    Searcher.SearchWordOfQuery(termToAdd2, p_query);
                    return true;
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
            string[] words = Cleantext[index].Split('-');
            MatchCollection termRange = range.Matches(Cleantext[index]);
            if (termRange.Count > 0)
            {
                foreach (Match v in termRange)
                {
                    Searcher.SearchWordOfQuery(v.ToString(), p_query);
                }
            }
            else
            {
                string value = Cleantext[index];
                //range: between _ and  _
                if (value == "between")
                {
                    if (index + 3 < Cleantext.Length)
                    {
                        if (Cleantext[index + 2] == "and")
                        {
                            string value4 = cleanGarbge(Cleantext[index + 3]);
                            string termToAdd = Cleantext[index] + " " + Cleantext[index + 1] + " " + Cleantext[index + 2] + " " + value4;
                            Searcher.SearchWordOfQuery(termToAdd, p_query);
                        }
                    }
                }
            }
            foreach (string w in words)
                checkWord(w);
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
