using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Task1
{
    public class MaskingWord
    {
        //private List<string> _badWordList = new List<string>();
        private static string copiedDir = @"..\..\..\ScannedFile\";
        
        int badCount = 0;

        public MaskingWord()
        {
            
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
        //cpoy a new file
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
            Task writer = null;

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

    }
}
