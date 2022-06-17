using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Path = System.IO.Path;

namespace Task1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string?> _badWordList = new List<string?>();
        
        private static string COPIEDDIR = @"..\..\..\ScannedFile\";

        private string _fileName = "";

        readonly MaskingWord _maskingWord = new MaskingWord();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _badWordList = _maskingWord.GetBadWords();
        }
        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(*.txt;*.docx)|*.txt;*.docx";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == true)
            {
                TextBoxBrowse.Text = fileDialog.FileName;
                _fileName = fileDialog.FileName;
            }
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_fileName != "") //scan the select file only if it browsed
            {
                ScanningForBadWord(_fileName, _badWordList);
               
                MessageBox.Show("Successfully scanned the file !", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }
            else // scan outside of the project folder if no directory or file selected
            {
                string[] files = null; 
                //files = Directory.GetFiles(@"..\..\..\..\..\..\..\","*.txt",SearchOption.TopDirectoryOnly);
                files = Directory.GetFiles(@"D:\","*.txt",SearchOption.TopDirectoryOnly);
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
                List<string?> maskedTexts = new List<string?>();
                
                var destFileName = Path.GetFileName(fileName);
                var file = new FileInfo(fileName);
                var destination = new FileInfo(COPIEDDIR + destFileName);

                maskedTexts = _maskingWord.GetMaskedTextList(fileName);

                if (maskedTexts.Count > 0)
                {
                    Task.Run(() =>
                    {
                        //First it copy the file to new directory if the bad word found
                        _maskingWord.CopyFile(file, destination, x => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBar.Value = x;
                            lblPercent.Content = x.ToString() + "%";
                        })));

                        //then mask the word
                        _maskingWord.MaskingWords(destination, maskedTexts);

                    }).GetAwaiter().OnCompleted(() => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressBar.Value = 100;
                        lblPercent.Content = "100%";
                        UpdateResultDisplay(_maskingWord.GetBadCount.ToString() + " bad words found in " + file.Name);
                    })));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
            }
        }
        
        private void UpdateResultDisplay(string word)
        {
            this.ListViewDisplay.Dispatcher.Invoke(delegate()
            {
                ListViewDisplay.Items.Add(word);
            });
        }

        private void UpdateReportDisplay(string word)
        {
            this.ListViewReport.Dispatcher.Invoke(delegate ()
            {
                ListViewReport.Items.Add(word);
            });
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            TextBoxBrowse.Clear();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            Task.WaitAll();
        }

        private void labelViewReport_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //new a list of report read from file as a method from MaskingWord

        }
    }
}
