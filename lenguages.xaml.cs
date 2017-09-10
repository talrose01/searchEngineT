using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SearchEngineP
{
    /// <summary>
    /// Interaction logic for lenguages.xaml
    /// </summary>
    public partial class lenguages : Window
    {
        public lenguages(MainWindow mw)
        {
            InitializeComponent();
            //foreach(string language in mw.languages)
            //{
            //    lenguagesWindow.Items.Add(language);
            //}
        }
    }
}
