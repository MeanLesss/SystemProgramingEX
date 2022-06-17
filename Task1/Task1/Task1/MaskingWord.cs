using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Task1
{
    public class MaskingWord
    {
        private List<string?> ReportList = new List<string?>();

        private static string BADWORDDIR = @"..\..\..\BadWordList\badwords.txt";
        private static string REPORTDIR = @"..\..\..\Report\Report.txt";
        private static string MASK = "*******";
        public int GetBadCount { get; set; }
        
        public MaskingWord()
        {
            
        }

        public void WriteReport()
        {

        }
        public List<string?> GetMaskedTextList(string? fileName )
        {
            List<string?> maskedTexts = new List<string?>();

            int badCount = 0;
            string line = "";
            StreamReader reader = new StreamReader(fileName);
            string maskWord = "";
            bool scannedLine = false;

            while (true)
            {
                line = reader.ReadLine();
                foreach (var badWord in GetBadWords())
                {
                    if (line != null)
                    {
                        if (line.Contains(badWord))
                        {
                            badCount++;
                            maskWord = line.Replace(badWord, MASK);
                            //need to write a new file back and masked the word
                            maskedTexts.Add(maskWord);
                            scannedLine = true;
                            break;
                        }
                    }
                }
                if (reader.EndOfStream)
                {
                    break;
                }

                if (!scannedLine)
                {
                    //need to write a new file back and masked the word
                    maskedTexts.Add(line);
                }

                scannedLine = false;
            }
            reader.Close();

            GetBadCount = badCount;
            return maskedTexts;
        }
        //masking the bad word
        public void MaskingWords(FileInfo fileName,List<string?> MaskedFile)
        {
            try
            {
                StreamWriter writer = new StreamWriter(fileName.FullName);

                foreach (var newLine in MaskedFile)
                {
                    writer.WriteLine(newLine);
                }
                writer.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("File error :" + ex.Message);
                Thread.EndCriticalRegion();
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
