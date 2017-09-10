using Microsoft.Win32;
using SearchEngineP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace SearchEngineP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool dictioneryInMemory;
        ReadFile rd;
        string path, postingPath, pathOfQueries;
        bool stemming;
        bool stemmingQuery;
        //****************************
        List<string> topFive;
        //****************************
        List<string> Selectedlanguages;
        string query;

        public MainWindow()
        {
            dictioneryInMemory = false;
            path = "";
            postingPath = "";
            stemming = false;
            stemmingQuery = false;
            Selectedlanguages = new List<string>();
            //CreateLanuagesList();
            InitializeComponent();
        }

        //public void CreateLanuagesList()
        //{
        //    languages = new List<string>();
        //    languages.Add("English");
        //    languages.Add("Afrikaans");
        //    languages.Add("Mandarin");
        //    languages.Add("French");
        //    languages.Add("Chinese");
        //    languages.Add("Hungarian");
        //    languages.Add("Tagalog");
        //    languages.Add("Polish");
        //    languages.Add("Japanese");
        //    languages.Add("Persian");
        //    languages.Add("Arabic");
        //    languages.Add("Serbo-Croatian");
        //    languages.Add("Portuguese");
        //    languages.Add("Spanish");
        //    languages.Add("Romanian");
        //    languages.Add("Czech");
        //    languages.Add("Georgian");
        //    languages.Add("German");
        //    languages.Add("Russian");
        //    languages.Add("Tajik");
        //    languages.Add("Estonian");
        //    languages.Add("Azeri");
        //    languages.Add("Dutch");
        //    languages.Add("Italian");
        //    languages.Add("Ukrainian");
        //    languages.Add("Armenian");
        //    languages.Add("Korean");
        //    languages.Add("Vietnamese");
        //    languages.Add("Hebrew");
        //    languages.Add("Bengali");
        //    languages.Add("Danish");
        //    languages.Add("Swedish");
        //    languages.Add("Greek");
        //    languages.Add("Slovene");
        //    languages.Add("Indonesian");
        //    languages.Add("Thai");
        //    languages.Add("Kazakh");
        //    languages.Add("Slovak");
        //    languages.Add("Cambodian");
        //    languages.Add("Pashto");
        //    languages.Add("Lao");
        //    languages.Add("Kyrgyz");
        //    languages.Add("Hindi");
        //    languages.Add("Urdu");
        //    //LenguagesList();
        //}

        //private void ListOfQuries(object sender, EventArgs e)
        //{
        //   List<string> nList = new List<string>();
        //    nList.Add("Space Program");
        //    nList.Add("Water Pollution");
        //    nList.Add("Genetic Engineering");
        //    nList.Add("International Terrorists");
        //    nList.Add("Impact of Government Regulated Grain Farming on International");
        //    nList.Add("Real Motives for Murder");
        //    nList.Add("Airport Security");
        //    nList.Add("Wildlife Extinction");
        //    nList.Add("piracy");
        //    nList.Add("Nobel prize winners");
        //    nList.Add("oceanographic vessels");
        //    nList.Add("Schengen agreement");
        //    nList.Add("Three Gorges Project");
        //    nList.Add("robotic technology");
        //    nList.Add("King Hussein, peace");
        //    //queriesList.DataSource = nList;
        //}
        /// <summary>
        /// get an event from the read file and show the messege box
        /// </summary>
        /// <param name="num">num of event</param>
        /// <param name="content">content of the messege</param>
        private void viewModelChanged(int num, string content)
        {
            if (num == 1)
            {
                System.Windows.MessageBox.Show(content);
            }
        }

        /// <summary>
        /// delete the Docs folders
        /// </summary>
        /// <param name="stemming">if stem</param>
        private void deleteDocs(bool stemming)
        {
            string path = "";
            if (stemming)
                path = postingPath + "\\SDocs";
            else
                path = postingPath + "\\Docs";
            if (Directory.Exists(path))
                Directory.Delete(path, true);


        }
        /// <summary>
        /// click to start the indexing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            stemming = Stemming.IsChecked.Value;
            postingPath = DataPostingPath_Box.Text;
            path = DataFilesPath_Box.Text;
            //case there is no path inside
            if (postingPath.Length == 0 && path.Length == 0)
            {
                System.Windows.MessageBox.Show("Data sets files path and posting files path are wrong.");
            }
            else if (path.Length == 0)
            {
                System.Windows.MessageBox.Show("Data sets files path is wrong.");

            }
            else if (postingPath.Length == 0)
            {
                System.Windows.MessageBox.Show("Posting files path is wrong.");
            }
            else
            {//case the path files
                if (!Directory.Exists(path))
                {
                    System.Windows.MessageBox.Show("The path for the data set files is not valid\n Please enter a new  path");
                }//case the postingPath files
                else if (!Directory.Exists(postingPath))
                {
                    System.Windows.MessageBox.Show("The path for the posting files is not valid\n Please enter a new  path");
                }//case both not exist
                else if (!Directory.Exists(path) && !Directory.Exists(postingPath))
                {
                    System.Windows.MessageBox.Show("The path for the data set files and the path for the posting files are not valid\n Please ente rnew paths");
                }
                // everything is ok.. should continue
                else
                {
                    deleteDocs(stemming);
                    deleteDocs(!stemming);
                    Directory.CreateDirectory(postingPath + "\\Docs");
                    Directory.CreateDirectory(postingPath + "\\SDocs");
                    rd = new ReadFile(path, postingPath, stemming);
                    rd.ModelChanged += viewModelChanged;
                    rd.startThreads();
                    //languages = rd.languageList;
                }
            }

        }

        /// <summary>
        /// click to browse posting path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_Posting_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browserPosting = new FolderBrowserDialog();
            if (browserPosting.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                DataPostingPath_Box.Text = browserPosting.SelectedPath;
            }
        }
        /// <summary>
        /// click to browse data set path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_Files_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browserFiles = new FolderBrowserDialog();
            if (browserFiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                DataFilesPath_Box.Text = browserFiles.SelectedPath;
            }
        }
        private void Brow_Posting_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browserFiles = new FolderBrowserDialog();
            if (browserFiles.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                DocsPostingPath_Box.Text = browserFiles.SelectedPath;
            }
        }

        /// <summary>
        /// click to delete the posing file and free the memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (DataPostingPath_Box.Text.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter Posting Files path");
            }
            else if (DataPostingPath_Box.Text.Length != 0)
            {
                if (!Directory.Exists(DataPostingPath_Box.Text))
                {
                    System.Windows.MessageBox.Show("Your posting files path is not valid, Please enter a new one.");

                }
                else
                {
                    try
                    {
                        Reset.IsEnabled = false;
                        //     deletePosting(!stemming);
                        // deletePosting(stemming);
                        deleteDocs(stemming);
                        deleteDocs(!stemming);
                        System.Windows.MessageBox.Show("The Posting Files have been removed");
                        Reset.IsEnabled = true;
                        rd = null;
                        // Start.IsEnabled = true;
                    }
                    catch (Exception)
                    {
                        Reset.IsEnabled = true;
                        System.Windows.MessageBox.Show("Somthing get wrong, The Posting Files have not been removed");
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// click to display the dictionery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_Click(object sender, RoutedEventArgs e)
        {
            stemming = Stemming.IsChecked.Value;
            postingPath = DataPostingPath_Box.Text;
            path = DataFilesPath_Box.Text;
            if (DataPostingPath_Box.Text.Length == 0)//case there is no path
            {
                System.Windows.MessageBox.Show("Please enter Posting Files path");
            }
            else//there is a path
            {//case its not valid
                if (!Directory.Exists(DataPostingPath_Box.Text))
                {
                    System.Windows.MessageBox.Show("Your posting files path is not valid, Please enter a new one.");
                }
                //case its not valid
                else
                { //case valid
                    if (rd == null)//called after reset or without building
                        rd = new ReadFile(path, postingPath, stemming);
                    if (!dictioneryInMemory)// case its without indexind...
                        rd.getTermsToDictionaryR();

                    DisplayPosting.Text = rd.getDisplayR();
                    System.Windows.MessageBox.Show("The Dictionery was loaded");
                   
                }
            }
        }
        /// <summary>
        /// click to load the dictionry to the memory of the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            stemming = Stemming.IsChecked.Value;
            postingPath = DataPostingPath_Box.Text;
            path = DataFilesPath_Box.Text;
            if (postingPath.Length == 0 && path.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter Data Sets Files path and Posting Files path");
            }
            else if (path.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter Data Sets Files path");

            }
            else if (postingPath.Length == 0)
            {
                System.Windows.MessageBox.Show("Please enter Posting Files path");
            }

            else
            {//case the path files
                if (!Directory.Exists(path))
                {
                    System.Windows.MessageBox.Show("The path for the data set files is not valid\n Please enter a new  path");
                }//case the postingPath files
                else if (!Directory.Exists(postingPath))
                {
                    System.Windows.MessageBox.Show("The path for the posting files is not valid\n Please enter a new  path");
                }//case both not exist
                else if (!Directory.Exists(path) && !Directory.Exists(postingPath))
                {
                    System.Windows.MessageBox.Show("The path for the data set files and the path for the posting files are not valid\n Please enter new paths");
                }
                // everything is ok.. should continue
                else
                {
                    System.Windows.MessageBox.Show("Your dictionery is loaded.\nThe process will take some seconds");
                    if (rd == null)
                        rd = new ReadFile(path, postingPath, stemming);
                    rd.getTermsToDictionaryR();
                    System.Windows.MessageBox.Show("The dictionery is in the memory!");
                    dictioneryInMemory = true;
                }

            }
        }
        /// <summary>
        /// check if stemming is required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stemming_Checked(object sender, RoutedEventArgs e)
        {
            stemming = true;
        }
        /// <summary>
        /// display all the languges of the corpus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void LenguagesList()
        //{
        //    foreach (string language in languages)
        //    {
        //        ListLan.Items.Add(language);
        //    }
        //}

        private void queriesList_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            //Searcher.path = path;
            ////Searcher.createRelevateDic();
            //Searcher.createStopWordsDic();
            //Searcher.ReadDocs();
            //ParseQuery.createMonthDic();
        }


        private void FilesPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Rankt_Click(object sender, RoutedEventArgs e)
        {
            if (DocsPostingPath_Box.Text == "")
            { 
                System.Windows.MessageBox.Show("Posting files path is wrong.");
            }
            else
            {//case the path files
                postingPath = DocsPostingPath_Box.Text;                
                 if (!Directory.Exists(postingPath))
                {
                    System.Windows.MessageBox.Show("The path for the posting files is not valid\n Please enter a new  path");
                }
                // everything is ok.. should continue
                else
                {
                    Searcher searcher = new Searcher(postingPath);
                    //Searcher.createRelevateDic();
                    Searcher.createStopWordsDic();
                    Searcher.ReadDocs();
                    ParseQuery.createMonthDic();
                    if (Query_Box.Text == "" && FilesPath.Text == "")
                    {
                        System.Windows.MessageBox.Show("Please enter Query Or a Path of Files of Quries!");
                    }
                    else
                    {
                        if (Query_Box.Text != "")
                        {
                            query = Query_Box.Text;
                            Dictionary<string, double> QureyScores = searcher.SearchSingleQuery(query, Selectedlanguages, stemmingQuery);
                            string s = "Top 50 Relevance Documents Of The Query: "+ query+":\n";
                            foreach (KeyValuePair<string, double> p in QureyScores)
                                s = s + p.Key + ": " + p.Value + "\n";
                            System.Windows.MessageBox.Show(s);
                        }
                        else
                        {
                            pathOfQueries = FilesPath.Text;
                            Dictionary<string, Dictionary<string, double>> QuriesScores = searcher.SearchFileOfQueries(pathOfQueries, Selectedlanguages, stemmingQuery);
                            foreach (KeyValuePair<string, Dictionary<string, double>> dic in QuriesScores)
                            {
                                string s = "Top 50 Relevance Documents Of The Query: " + dic.Key + ":\n";
                                foreach (KeyValuePair<string, double> p in dic.Value)
                                {
                                    s = s + p.Key + ": " + p.Value + "\n";
                                }                                   
                                System.Windows.MessageBox.Show(s);
                            }
                            
                        }
                    }
                }
            }
        }
        private void txt_changed_Click(object sender, TextChangedEventArgs e)
        {
            string s = Query_Box.Text;

            if ((s.Length != 0) && (s[s.Length - 1] == ' ') && (s.Length != 1))
            {
                //System.Windows.MessageBox.Show("dasdas");
                topFive = new List<string>();
                createTopFive(ref topFive, s);
                foreach (string item in topFive)
                {
                    complete_term.Items.Add(item);
                }
            }

        }
        private void createTopFive(ref List<string> list, string text)
        {
            list.Add("aaa");
            list.Add("bbb");
            list.Add("ccc");
            list.Add("ddd");
            list.Add("eee");
        }

        private void add_term(object sender, SelectionChangedEventArgs e)
        {
            Query_Box.Text = Query_Box.Text + complete_term.SelectedItem;
            complete_term.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// clean the dicitionry display from the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanTheScreen_Click(object sender, RoutedEventArgs e)
        {
            DisplayPosting.Text = " ";
        }

        //languages
        private void CheckBox_English(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("English");
        }

        private void CheckBox_Afrikaans(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Afrikaan");
        }

        private void CheckBox_Mandarin(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Mandarin");
        }

        private void CheckBox_Chinese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Chinese");
        }

        private void CheckBox_Hungarian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Hungarian");
        }

        private void CheckBox_French(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("French");
        }

        private void CheckBox_Tagalog(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Tagalog");
        }

        private void CheckBox_Polish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Polish");
        }

        private void CheckBox_Japanese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Japanese");
        }

        private void CheckBox_Persian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Persian");
        }

        private void CheckBox_Arabic(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Arabic");
        }

        private void CheckBox_SerboCroatian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Serbo-Croatian");
        }

        private void CheckBox_Portuguese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Portuguese");
        }

        private void CheckBox_Spanish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Spanish");
        }

         private void CheckBox_Romanian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Romanian");
        }

        private void CheckBox_Czech(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Czech");
        }

        private void CheckBox_Georgian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Georgian");
        }

        private void CheckBox_German(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("German");
        }

        private void CheckBox_Russian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Russian");
        }

        private void CheckBox_Tajik(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Tajik");
        }

        private void CheckBox_Estonian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Estonian");
        }

        private void CheckBox_Azeri(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Azeri");
        }

        private void CheckBox_Dutch(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Dutch");
        }

        private void CheckBox_Italian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Italian");
        }

        private void CheckBox_Ukrainian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Ukrainian");
        }

        private void CheckBox_Armenian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Armenian");
        }

        private void CheckBox_Korean(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Korean");
        }

        private void CheckBox_Vietnamese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Vietnamese");
        }
        private void CheckBox_Hebrew(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Hebrew");
        }

        private void CheckBox_Bengali(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Bengali");
        }

        private void CheckBox_Danish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Danish");
        }

        private void CheckBox_Swedish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Swedish");
        }

        private void CheckBox_Greek(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Greek");
        }
        private void CheckBox_Slovene(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Slovene");
        }

        private void CheckBox_Indonesian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Indonesian");
        }

        private void CheckBox_Thai(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Thai");
        }

        private void CheckBox_Kazakh(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Kazakh");
        }
        private void CheckBox_Slovak(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Slovak");
        }
        private void CheckBox_Cambodian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Cambodian");
        }

        private void CheckBox_Pashto(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Pashto");
        }

        private void CheckBox_Lao(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Lao");
        }

        private void CheckBox_Kyrgyz(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Kyrgyz");
        }
        private void CheckBox_Hindi(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Hindi");
        }

        private void CheckBox_Urdu(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Add("Urdu");
        }

        //uncheck
        private void Un_English(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("English");
        }

        private void Un_Afrikaans(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Afrikaan");
        }

        private void Un_Mandarin(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Mandarin");
        }

        private void Un_Chinese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Chinese");
        }

        private void Un_Hungarian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Hungarian");
        }

        private void Un_French(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("French");
        }

        private void Un_Tagalog(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Tagalog");
        }

        private void Un_Polish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Polish");
        }

        private void Un_Japanese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Japanese");
        }

        private void Un_Persian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Persian");
        }

        private void Un_Arabic(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Arabic");
        }

        private void Un_SerboCroatian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Serbo-Croatian");
        }

        private void Un_Portuguese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Portuguese");
        }

        private void Un_Spanish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Spanish");
        }

        private void Un_Romanian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Romanian");
        }

        private void Un_Czech(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Czech");
        }

        private void Un_Georgian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Georgian");
        }

        private void Un_German(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("German");
        }

        private void Un_Russian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Russian");
        }

        private void Un_Tajik(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Tajik");
        }

        private void Un_Estonian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Estonian");
        }

        private void Un_Azeri(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Azeri");
        }

        private void Un_Dutch(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Dutch");
        }

        private void Un_Italian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Italian");
        }

        private void Un_Ukrainian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Ukrainian");
        }

        private void Un_Armenian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Armenian");
        }

        private void Un_Korean(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Korean");
        }

        private void Un_Vietnamese(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Vietnamese");
        }
        private void Un_Hebrew(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Hebrew");
        }

        private void Un_Bengali(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Bengali");
        }

        private void Un_Danish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Danish");
        }

        private void Un_Swedish(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Swedish");
        }

        private void Un_Greek(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Greek");
        }
        private void Un_Slovene(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Slovene");
        }

        private void Un_Indonesian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Indonesian");
        }

        private void Un_Thai(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Thai");
        }

        private void Un_Kazakh(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Kazakh");
        }
        private void Un_Slovak(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Slovak");
        }
        private void Un_Cambodian(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Cambodian");
        }

        private void Un_Pashto(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Pashto");
        }

        private void Un_Lao(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Lao");
        }

        private void Un_Kyrgyz(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Kyrgyz");
        }
        private void Un_Hindi(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Hindi");
        }

        private void Un_Urdu(object sender, RoutedEventArgs e)
        {
            Selectedlanguages.Remove("Urdu");
        }


        private void ListLan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StemmingQuery_Checked(object sender, RoutedEventArgs e)
        {
            stemmingQuery = true;
        }


        
    }
}
