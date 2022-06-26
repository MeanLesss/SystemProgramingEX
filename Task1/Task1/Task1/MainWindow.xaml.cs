using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Printing;
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
        static CancellationTokenSource source = new CancellationTokenSource();
        static CancellationToken token = source.Token;
        TaskFactory taskFactory = new TaskFactory(token);
        Mutex _mutex = new Mutex();


        private List<BadWord?> _badWordList = new List<BadWord?>();

        private static readonly string COPIEDDIR = @"..\..\..\ScannedFile\";
        

        readonly MaskingWord _maskingWord = new MaskingWord();
        
        public MainWindow()
        {
            
            InitializeComponent();

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _badWordList = _maskingWord.GetBadWordRank();

            UpdateTopList();
        }
        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            labelProcess.Content = " ";
            lblPercent.Content = "0%";
            progressBar.Value = 0;
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "(*.txt;*.docx)|*.txt;*.docx",
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                TextBoxBrowse.Text = fileDialog.FileName;
            }
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            labelProcess.Content = " ";
            lblPercent.Content = "0%";
            progressBar.Value = 0;
            ButtonStart.IsEnabled = false;
            ButtonPause.IsEnabled = true;
            if (TextBoxBrowse.Text != "") //scan the select file only if it browsed
            {
                ScanningForBadWord(TextBoxBrowse.Text, _badWordList);
            }
            else // scan Drive D:\ outside of the project folder if no directory or file selected
            {
                string[] files = {""};
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
            }
        }
        public void ScanningForBadWord(string fileName, List<BadWord?> badWordList)
        {
            try
            {
                var maskedTexts = new List<string?>();
                var destFileName = Path.GetFileName(fileName);
                var file = new FileInfo(fileName);
                var destination = new FileInfo(COPIEDDIR + destFileName);
                maskedTexts = _maskingWord.GetMaskedTextList(file,_badWordList);
                //copy file when it found bad word
                Thread copyThread = new Thread(copyThread => _maskingWord.CopyFile(file, destination,
                    x => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        labelProcess.Content = "Copying file";
                        progressBar.Value = x;
                        lblPercent.Content = x.ToString() + "%";
                    }))));
                //then mask the word
                Thread maskingWordsThread = new Thread(masking => _maskingWord.MaskingWords(destination, maskedTexts,
                    x =>progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        labelProcess.Content = "Masking word";
                        progressBar.Value = x;
                        lblPercent.Content = x.ToString() + "%";
                    }))));

                //making report
                Thread threadReport = new Thread(x => _maskingWord.WriteReport(
                    x=>progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        labelProcess.Content = "Making report";
                        progressBar.Value = x;
                        lblPercent.Content = x.ToString() + "%";
                    }))));


                if (maskedTexts.Count > 0)
                {
                    Task.Run(() =>
                    {
                        this.labelProcess.Dispatcher.BeginInvoke(() => { labelProcess.Content = "Copying file"; });
                        //First it copy the file to new directory if the bad word found and display loading progress
                        _mutex.WaitOne();
                        copyThread.Start(source.Token);
                        copyThread.Join();
                        _mutex.ReleaseMutex();

                        Thread.Sleep(1000);
                        ResetProgressBar();
                        _mutex.WaitOne();
                        maskingWordsThread.Start();
                        maskingWordsThread.Join();
                        _mutex.ReleaseMutex();

                        Thread.Sleep(1000);
                        ResetProgressBar();
                        //this.labelProcess.Dispatcher.BeginInvoke(() => { labelProcess.Content = "Making report"; });
                        _mutex.WaitOne();
                        threadReport.Start();
                        threadReport.Join();
                        _mutex.ReleaseMutex();

                    }).GetAwaiter().OnCompleted(() => progressBar.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //if the file is too small it just gonna jump here cuz the calculation is at 0.0sth so
                        //the return progress is still 0
                        progressBar.Value = 100;
                        lblPercent.Content = "100%";
                        UpdateResultDisplay(_maskingWord.GetBadCount.ToString() + " bad words found in " + file.Name);
                        ButtonStart.IsEnabled = true;
                        ButtonPause.IsEnabled = false;
                        ButtonResume.IsEnabled = false;
                        //refresh the old list
                        _badWordList.Clear();
                        _badWordList = _maskingWord.GetBadWordRank();
                        _maskingWord.WriteBadWordReport(_badWordList);// make this a thread too and make progress
                        UpdateTopList();
                    })));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
            }
        }

        private void ResetProgressBar()
        {
            this.progressBar.Dispatcher.BeginInvoke(delegate()
            {
                progressBar.Value = 0;
            });
            this.labelProcess.Dispatcher.BeginInvoke(delegate()
            {
                labelProcess.Content = " ";
            });
            this.lblPercent.Dispatcher.BeginInvoke(delegate()
            {
                lblPercent.Content = "0%";
            });
        }

        private void UpdateTopList()
        {
            ListViewTopWord.Items.Clear();
            int i = 1;
            foreach (var word in _maskingWord.GetTopWord(_badWordList))
            {
                ListViewTopWord.Items.Add("Top " + i + " : " + word.Count + " - " + word.Word);
                i++;
            }
            i = 0;
        }
        private void UpdateResultDisplay(string? word)
        {
            this.ListViewDisplay.Dispatcher.Invoke(delegate()
            {
                ListViewDisplay.Items.Add(word);
            });
        }

        private void UpdateReportDisplay(string? word)
        {
            this.ListViewReport.Dispatcher.Invoke(delegate ()
            {
                ListViewReport.Items.Add(word);
            });
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            TextBoxBrowse.Clear();
            labelProcess.Content = " ";
            lblPercent.Content = "0%";
            progressBar.Value = 0;
            source.Cancel();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            ButtonPause.IsEnabled = false;
            ButtonResume.IsEnabled = true;
            _mutex.WaitOne();
        }
        private void ButtonResume_Click(object sender, RoutedEventArgs e)
        {
            ButtonPause.IsEnabled = true;
            ButtonResume.IsEnabled = false;
            _mutex.ReleaseMutex();
        }
        private void labelViewReport_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //new a list of report read from file as a method from MaskingWord
            ListViewReport.Items.Clear();
            foreach (var line in _maskingWord.GetReportList())
            {
                UpdateReportDisplay(line);
            }
        }

       
    }
}
