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
using Path = System.IO.Path;

namespace Task1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string?> _badWordList = new List<string?>();

        private static string BADWORDDIR = @"..\..\..\BadWordList\badwords.txt";
        private static string COPIEDDIR = @"..\..\..\ScannedFile\";
        private static string MASK = "*******";

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
            if (fileName != null) //scan the select file only if it browsed
            {
                ScanningForBadWord(fileName, _badWordList);
               
                MessageBox.Show("You have successfully copied the file !", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }
            else // scan outside of the project folder if no directory or file selected
            {
                string[] files = null;
                files = Directory.GetFiles(@"..\..\..\..\..\..\..\","*.txt",SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        ScanningForBadWord(fi.FullName.ToString(),_badWordList);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message);
                        continue;
                    }
                }
                MessageBox.Show("Successfully scanned the file !", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
        public void ScanningForBadWord(string fileName, List<string?> badWordList)
        {
            try
            {
                int badCount = 0;
                string line = "";
                List<string?> MaskedFile = new List<string?>();

                StreamReader reader = new StreamReader(fileName);
                string maskWord = "";
                bool scannedLine = false;

                while (true)
                {
                    line = reader.ReadLine();
                    foreach (var badWord in badWordList)
                    {
                        if (line.Contains(badWord))
                        {
                            badCount++;
                            maskWord = line.Replace(badWord, MASK);
                            //need to write a new file back and masked the word
                            MaskedFile.Add(maskWord);
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
                        //need to write a new file back and masked the word
                        MaskedFile.Add(line);
                    }

                    scannedLine = false;
                }
                reader.Close();

                var destFileName = Path.GetFileName(fileName);
                var file = new FileInfo(fileName);
                var destination = new FileInfo(COPIEDDIR + destFileName);

                MaskingWord MaskingWord = new MaskingWord();

                if (badCount > 0)
                {

                    Task.Run(() =>
                    {
                        //First it copy the file to new directory if the bad word found
                        MaskingWord.CopyFile(file, destination, x => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBar.Value = x;
                            lblPercent.Content = x.ToString() + "%";
                        })));

                        //then mask the word
                        MaskingWord.MaskingWords(destination, MaskedFile);

                    }).GetAwaiter().OnCompleted(() => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressBar.Value = 100;
                        lblPercent.Content = "100%";
                        UpdateDisplay(badCount.ToString() + " bad words found in " + file.Name);
                    })));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
            }
        }
        
        private void UpdateDisplay(string word)
        {
            this.ListViewDisplay.Dispatcher.Invoke(delegate()
            {
                ListViewDisplay.Items.Add(word);
            });
        }
        private static List<string?> GetBadWords()
        {
            var badWords = new List<string?>();
            try
            {
                StreamReader streamReader = new StreamReader(BADWORDDIR);
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    badWords.Add(line);
                }
                streamReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error file : not found" + ex.Message);
            }


            return badWords;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            TextBoxBrowse.Clear();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            Task.WaitAll();
        }
    }
}
