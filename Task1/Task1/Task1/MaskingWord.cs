using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps.Serialization;

namespace Task1
{
    public class MaskingWord
    {
        private List<BadWord> _badWordList = new List<BadWord>();
        private readonly List<BadWord?> _badWordReport = new List<BadWord?>();

        private readonly List<string?>  _foundDir = new List<string?>();

        private static string BADWORDDIR = @"..\..\..\BadWordList\badwords.txt";
        private static string BADWORDDIRRANK = @"..\..\..\BadWordList\badwordrank.txt";
        private static string TESTFILE = @"..\..\..\BadWordList\Test.txt";
        private static string REPORTDIR = @"..\..\..\Report\Report.txt";
        private static string TEMPDIR = @"..\..\..\Report\Temp.txt";
        private static string MASK = "*******";
        public int GetBadCount { get; set; }
        
        public MaskingWord()
        {
            
        }

        public List<BadWord> GetBadWordReport()
        {
            var badwordReport = _badWordReport.GroupBy(x => x.Word)
                .Select(g => new BadWord()
                {
                    Word = g.Key,
                    Count = g.Select(x => x.Count).Max()
                });

            return badwordReport.ToList();
        }

        public void WriteBadWordReport (List<BadWord?> oldList)
        {
            if (!File.Exists(BADWORDDIRRANK))
            {
                FileStream fs = File.Create(BADWORDDIRRANK);
                fs.Close();
            }
            // still cant find the problem why it double?????????????????????

            //var oldList = GetBadWordRank();
            GetBadWordReport().ForEach(oldList.Add);

            var newList = oldList.GroupBy(x => x.Word).Select(
                g => new BadWord()
                {
                    Word = g.Key,
                    Count = g.Select(x => x.Count).Max()
                });

            StreamWriter sw = new StreamWriter(BADWORDDIRRANK);

            foreach (var word in newList.ToList())
            {
                sw.WriteLine(word.Count + ":" + word.Word);
            }
            sw.Close();

        }

        public void WriteReport(Action<int> progressCallBack)
        {
            //add file replace in a list tag(#FILE_REPLACE) future update

            //to sort and pick top 10 from the list  but NO clue how to count for each word that found (future update)
            /*List<int> list = new List<int>();
            var result = list.OrderByDescending(w => w).Take(10);*/

            if (!File.Exists(REPORTDIR))
            {
                FileStream fs = File.Create(REPORTDIR);
                fs.Close();
            }

            //GET ALL REPORT FROM THE REPORT FILE
            var reportList = GetReportList();

            //combine list
            _foundDir.ForEach(found => reportList.Add(found));

            //WRITE TO OLD REPORT
            StreamWriter sw = new StreamWriter(REPORTDIR);


            int progress = 0;
            int i = 0;
            int reportedProgress = 0;
            foreach (var report in reportList)
            {
                sw.WriteLine(report);
                //progress part
                float stringLen = i;
                float maskFileLen = reportList.Count;

                if ((progress = (int)((stringLen / maskFileLen) * 100)) != reportedProgress)
                    progressCallBack(reportedProgress = progress);
                i++;
            }

            sw.Close();
            reportList.Clear();
            _foundDir.Clear();
            
            ////METHOD COPY TEMP TO REPORT BUT FOR FUTURE UPDATE
            /*
            File.Copy(TEMPDIR, REPORTDIR, true);*/
            //File.Delete(TEMPDIR);

        }


        public List<string?> GetReportList()
        {
            List<string?> reportList = new List<string?>();
            try
            {
                StreamReader reader = new StreamReader(REPORTDIR);
                while (true)
                {
                    reportList.Add(reader.ReadLine());
                    if (reader.EndOfStream) break;
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return reportList;
        }
        public List<string?> GetMaskedTextList(FileInfo fileName,List<BadWord?> getBadWord)
        {
            List<string?> maskedTexts = new List<string?>();

            int badCount = 0;

            StreamReader reader = new StreamReader(fileName.FullName);

            while (true) //read everything from the copied file that have bad word
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    List<string?>? subs = line.Split(" ").ToList()!;
                    foreach (var sub in subs)
                    {
                        //var getBadWord = GetBadWordRank();

                        //filtering text for special characters
                        if (sub.Contains('.') || sub.Contains(','))
                        {
                            string remove = "";
                            if (sub.Contains('.'))
                            {
                                remove = sub.Remove(sub.IndexOf('.'));
                            }

                            if (sub.Contains(','))
                            {
                                remove = sub.Remove(sub.IndexOf(','));
                                
                            }
                            
                            //search for bad word 
                            BadWord? badword = getBadWord.Find(x => remove.Contains(x.Word));
                            
                            //add everything into a new list
                            if (badword != null)
                            {
                                badCount++;
                                //count bad word and add to the report list
                                badword.Count++;
                                _badWordReport.Add(badword);

                                var maskWord = sub.Replace(badword.Word, MASK).ToString();
                                maskedTexts.Add(maskWord);

                            }
                            else
                            {
                                maskedTexts.Add(sub);
                            }
                        }
                        else
                        {
                            BadWord? badword = getBadWord.Find(x => sub.Contains(x.Word));
                            if (badword != null)
                            {
                                badCount++;
                                badword.Count++;
                                _badWordReport.Add(badword);
                                var maskWord = sub.Replace(badword.Word, MASK).ToString();
                                maskedTexts.Add(maskWord);
                            }
                            else
                            {
                                maskedTexts.Add(sub);
                            }
                        }
                    }
                }

                if (reader.EndOfStream)
                {
                    break;
                }

            }
            reader.Close();

            //pass the value to a list for WriteReport()
            if (maskedTexts.Count > 0)
            {
                var reportDir = fileName.FullName + "\t|" + fileName.Length + " bytes\t | " + badCount + " words found";
                _foundDir.Add(reportDir);
            }
            GetBadCount = badCount;
            return maskedTexts;
        }

        //masking the bad word
        public void MaskingWords(FileInfo destination, List<string?> MaskedFile, Action<int> progressCallBack)
        {
            try
            {
                int progress = 0;
                int i = 0;
                int reportedProgress = 0;
                StreamWriter writer = new StreamWriter(destination.FullName);
                foreach (var newLine in MaskedFile) //write everything in MaskedFile that got from GetMaskedTextFile passed from mainWindow
                {
                    writer.Write(newLine + " ");
                    //progress part
                    float stringLen = i;
                    float maskFileLen = MaskedFile.Count;
                 
                    if ((progress = (int)((stringLen / maskFileLen) * 100)) != reportedProgress)
                        progressCallBack(reportedProgress = progress);
                    i++;
                }
                writer.Close();
            }
            catch (Exception ex)
            {

            }
        }

        //copy a new file
        public void CopyFile(FileInfo file, FileInfo destination,Action<int> progressCallback) //file is the current file
        {
            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize], buffer2 = new byte[bufferSize];
            bool swap = false;
            int progress = 0, reportedProgress = 0, read = 0;
            float len = file.Length;
            float flen = len;
            Task writer = null;
            using (var source = file.OpenRead())
            using (var dest = destination.OpenWrite())
            {
                dest.SetLength(source.Length);
                for (float size = 1; size < len; size += read)
                {
                    if ((progress = (int)((size / flen) * 100000000)) != reportedProgress)
                        progressCallback(reportedProgress = progress);
                    read = source.Read(swap ? buffer : buffer2, 0, bufferSize);
                    writer?.Wait();
                    writer = dest.WriteAsync(swap ? buffer : buffer2, 0, read);
                    swap = !swap;
                }
                writer?.Wait();
            }
        }

        //need to modify to display only 10 on start up
        public List<BadWord?> GetBadWordRank()
        {
            var getBadWordRank = new List<BadWord?>();
            try
            {
                StreamReader streamReader = new StreamReader(BADWORDDIRRANK);
                string? line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    BadWord? badWord = new BadWord();
                    
                    if (line.Contains(":"))
                    {
                        badWord.Count = int.Parse(line.Split(':')[0]);
                        badWord.Word = line.Split(':')[1];
                    }
                    getBadWordRank.Add(badWord);
                }
                streamReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error file : not found" + ex.Message);
            }

            return getBadWordRank;
        }
        
        //gat all bad word from the textfile and put it all in a list 
        public List<BadWord?> GetBadWords()
        {
            var badWords = new List<BadWord?>();
            try
            {
                StreamReader streamReader = new StreamReader(BADWORDDIR);
                string? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    BadWord? badWord = new BadWord();
                    badWord.Word = line;
                    badWords.Add(badWord);
                }
                streamReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error file : not found" + ex.Message);
            }
            return badWords;
        }
    }
}
