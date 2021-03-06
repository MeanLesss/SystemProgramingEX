using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task3
{
    internal class Program
    {
        public static List<int> OddNumber = new List<int>();

        static void Main(string[] args)
        {

            Mutex mutex = new Mutex();

            Thread[] threads = new Thread[1];
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i] = new Thread(UpdateFields);
                threads[i].Start();
            }

            for (int i = 0; i < threads.Length; ++i)
                threads[i].Join();

            Console.WriteLine("Process 1:");
            foreach (var odd in OddNumber)
            {
                Console.WriteLine(odd);
            }

            mutex.WaitOne();
            Process process = new Process();
            process.StartInfo.FileName = "Process2.exe";
            process.Start();
            process.WaitForExit();
            mutex.ReleaseMutex();


            Console.Read();
        }

        int count;

        public static void UpdateFields()
        {
            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 != 0)
                {
                    OddNumber.Add(i);
                }
            }

            for (int i = 10; i > 0; i--)
            {
                if (i % 2 != 0)
                {
                    OddNumber.Add(i);
                }
            }
        }
    }
}
