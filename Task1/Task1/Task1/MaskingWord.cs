using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Task1
{
    public class MaskingWord
    {
        private List<BadWord> _badWords = new List<BadWord>();
        private List<string?>  _foundDir = new List<string?>();

        private static string BADWORDDIR = @"..\..\..\BadWordList\badwords.txt";
        private static string REPORTDIR = @"..\..\..\Report\Report.txt";
        private static string TEMPDIR = @"..\..\..\Report\Temp.txt";
        private static string MASK = "*******";
        public int GetBadCount { get; set; }
        
        public MaskingWord()
        {
            
        }

        public void WriteReport()
        {
            //add file replace in a list tag(#FILE_REPLACE) future update

            //to sort and pick top 10 from the list  but NO clue how to count for each word that found (future update)
            List<int> list = new List<int>();
            var result = list.OrderByDescending(w => w).Take(10);
            

            //TEMP FILE IS JUST FOR FUTURE UPDATE TO SO NO DUPLICATE DIR SHOULD BE WRITE
            using (FileStream fs = File.Create(TEMPDIR))
            {
                fs.Close();
                //GET ALL REPORT FROM THE REPORT FILE
                var reportList = GetReportList();
                //WRITE TO REPORT
                StreamWriter sw = new StreamWriter(REPORTDIR);

                foreach (var line in reportList)
                {
                    sw.WriteLine(line);
                }

                //WRITE FROM LIST OF _foundDir THAT LOCATE IN GetMaskedTextList to save what found 
                foreach (var found in _foundDir)
                {
                    sw.WriteLine(found);
                }

                sw.Close();
                reportList.Clear();
                _foundDir.Clear();
            }


            ////METHOD COPY TEMP TO REPORT BUT FOR FUTURE UPDATE
            /*
            File.Copy(TEMPDIR, REPORTDIR, true);*/
            File.Delete(TEMPDIR);
            
        }

        public List<string?> GetReportList()
        {
            List<string?> reportList = new List<string?>();

            StreamReader reader = new StreamReader(REPORTDIR);
            while (true)
            {
                reportList.Add(reader.ReadLine());
                if(reader.EndOfStream) break;
            }
            reader.Close();
            return reportList;
        }
        public List<string?> GetMaskedTextList(FileInfo fileName )
        {
            List<string?> maskedTexts = new List<string?>();

            int badCount = 0;
            string line = "";
            StreamReader reader = new StreamReader(fileName.FullName);
            string maskWord = "";
            bool scannedLine = false;
            bool scannedSub = false;
            string[] subs = { "" };
            string subWord = "";
            string foundBadWord = "";
            while (true) //read everything from the copied file that have bad word
            {
                line = reader.ReadToEnd();
                if (line != null)
                {
                    subs = line.Split(' ');
                    foreach (var sub in subs)
                    {
                        if (sub != foundBadWord)
                        {
                            maskedTexts.Add(sub);
                        }

                        if (scannedSub == false)
                        {
                            foreach (var badWord in GetBadWords())
                            {
                                if (sub.Contains(badWord))
                                {
                                    badCount++;
                                    maskWord = sub.Replace(badWord, MASK).ToString(); //mask the word
                                    maskedTexts.Add(maskWord);
                                    foundBadWord = badWord;
                                    scannedLine = true;
                                    scannedSub = true;
                                }

                                if (scannedSub == true)
                                {
                                    scannedSub = false;
                                    break;
                                }
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
        public void MaskingWords(FileInfo destination,List<string?> MaskedFile)
        {
            try
            {
                StreamWriter writer = new StreamWriter(destination.FullName); 

                foreach (var newLine in MaskedFile)//write everything in MaskedFile that got from GetMaskedTextFile passed from mainWindow
                {
                    writer.Write(newLine + " ");
                }
                writer.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
            }
        }
        //copy a new file
        public void CopyFile(FileInfo file, FileInfo destination, Action<int> progressCallback)//file is the current file
        {
            const int bufferSize = 20000 * 20000;
            byte[] buffer = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            bool swap = false;
            int reportedProgress = 0;
            int read;
            long len = file.Length;
            float flen = len;
            Task? writer = null;

            using var source = file.OpenRead();
            using var dest = destination.OpenWrite();
            dest.SetLength(source.Length);

            for (long size = 0; size < len; size += read)
            {
                int progress;
                if ((progress = ((int)((size / flen) * 100))) != reportedProgress)
                    progressCallback(reportedProgress = progress);
                read = source.Read(swap ? buffer : buffer2, 0, bufferSize);
                writer?.Wait();
                writer = dest.WriteAsync(swap ? buffer : buffer2, 0, read);
                swap = !swap;
            }
            writer?.Wait();
        }

        //gat all bad word from the textfile and put it all in a list 
        public List<string?> GetBadWords()
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
    }
}
