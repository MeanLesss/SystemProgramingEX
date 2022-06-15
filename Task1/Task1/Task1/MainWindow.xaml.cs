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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Windows.Resources;

namespace Task1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _badWordList = new List<string>();

        private static string badWordDir = @"..\..\..\BadWordList\badwords.txt";
        private static string ScannedDir = @"..\..\..\ScannedFile\";
        private static string mask = "*******";

        private string fileName;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _badWordList = GetBadWords();
        }
        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(*.txt;*.docx)|*.txt;*.docx";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == true)
            {
                TextBoxBrowse.Text = fileDialog.FileName;
                fileName = fileDialog.FileName;
            }
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            //end here last night

            Thread startProgress = new Thread(new ThreadStart(MaskingWord));
            startProgress.Start();
            
        }

        private void MaskingWord()
        {
            int badCount = 0;
            string line = "";
            char[] lineArr = new char[20000];
            string maskWord = "";
            bool scannedLine = false;
            try
            {
                StreamReader reader = new StreamReader(fileName);

                while (true)
                {
                    line = reader.ReadLine();
                    foreach (var badWord in _badWordList)
                    {
                        if (line.Contains(badWord))
                        {
                            badCount++;
                            maskWord = line.Replace(badWord, mask) + "\n";
                            UpdateDisplay(maskWord);
                            scannedLine = true;
                            break;
                        }
                    }
                    if (reader.EndOfStream)
                    {
                        break;
                    }

                    if (!scannedLine)
                    {
                         UpdateDisplay(line);
                    }

                    scannedLine = false;
                }

                MessageBox.Show(badCount.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
            }
        }

        private void UpdateDisplay(string word)
        {
            this.TextBoxDisplay.Dispatcher.Invoke(delegate()
            {
                TextBoxDisplay.Text += word + "\n";
            });
        }
        private List<string> GetBadWords()
        {
            List<string> badWords = new List<string>();
            
            string line = "";
            try
            {
                StreamReader sr = new StreamReader(badWordDir);
                while ((line = sr.ReadLine()) != null)
                {
                    badWords.Add(line);
                }
                sr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error file : not found" + ex.Message);
            }
            

            return badWords;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
