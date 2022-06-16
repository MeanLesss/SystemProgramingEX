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
    public class FileCopy
    {
        //private List<string> _badWordList = new List<string>();
        private static string copiedDir = @"..\..\..\ScannedFile\";
        private static string mask = "*******";
        
        int badCount = 0;

        public FileCopy()
        {
            
        }
        
        //masking the bad word
        public void MaskingWord(string fileName,List<string> badWordList)
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
                    foreach (var badWord in badWordList)
                    {
                        if (line.Contains(badWord))
                        {
                            badCount++;
                            maskWord = line.Replace(badWord, mask) + "\n";
                            //need to write a new file back and masked the word

                            //UpdateDisplay(maskWord);
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
                        //UpdateDisplay(line);
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
        //create a new text with masked text
        public void CreateNewFile()
        {

        }
    }
}
