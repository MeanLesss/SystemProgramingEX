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
        private List<string> _badWordList = new List<string>();

        private static string badWordDir = @"..\..\..\BadWordList\badwords.txt";
        private static string copiedDir = @"..\..\..\ScannedFile\";
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
            ScanningForBadWord(fileName,_badWordList);
        }

        public void ScanningForBadWord(string fileName, List<string> badWordList)
        {
            try
            {
                int badCount = 0;
                string line = "";
                StreamReader reader = new StreamReader(fileName);

                while (true)
                {
                    line = reader.ReadLine();
                    foreach (var badWord in badWordList)
                    {
                        if (line.Contains(badWord))
                        {
                            badCount++;
                        }
                    }

                    if (reader.EndOfStream)
                    {
                        break;
                    }
                }

                reader.Close();

                if (badCount > 0)
                {
                    var destFileName = Path.GetFileName(fileName);
                    var file = new FileInfo(fileName);
                    var destination = new FileInfo(copiedDir + destFileName);
                    Task.Run(() =>
                    {
                        Copyfile(file, destination, x => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBar.Value = x;
                            lblPercent.Content = x.ToString() + "%";
                        })));
                    }).GetAwaiter().OnCompleted(() => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressBar.Value = 100;
                        lblPercent.Content = "100%";
                        MessageBox.Show("You have successfully copied the file !", "Message", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    })));

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
            }
        }

        public void Copyfile(FileInfo file, FileInfo destination, Action<int> progressCallback)//file is the current file
        {

            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize], buffer2 = new byte[bufferSize];
            bool swap = false;
            int progress = 0, reportedProgress = 0, read = 0;
            long len = file.Length;
            float flen = len;
            Task writer = null;
            using (var source = file.OpenRead())
            using (var dest = destination.OpenWrite())
            {
                dest.SetLength(source.Length);
                for (long size = 0; size < len; size += read)
                {
                    if ((progress = ((int)((size / flen) * 100))) != reportedProgress)
                        progressCallback(reportedProgress = progress);
                    read = source.Read(swap ? buffer : buffer2, 0, bufferSize);
                    writer?.Wait();
                    writer = dest.WriteAsync(swap ? buffer : buffer2, 0, read);
                    swap = !swap;
                }
                writer?.Wait();
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
