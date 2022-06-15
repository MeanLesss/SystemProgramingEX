using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task2
{
    internal class Program
    {
        private static readonly List<int> OddList = new List<int>();
        private static readonly List<int> EvenList = new List<int>();
        static void Main(string[] args)
        {
            Thread[] threads = new Thread[2];
            
            threads[0] = new Thread(GetOddNumber);
            threads[0].Start();
            
            threads[1] = new Thread(GetEvenNumber);
            threads[1].Start();
            

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("thread 1 : |\tthread 2 :");
            for (int i = 0; i < OddList.Count; i++)
            {
                Console.Write(OddList[i] + "\n\t\t");
                Console.WriteLine(EvenList[i]);
            }
            
            
            Console.Read();
            OddList.Clear();
            EvenList.Clear();
        }

        private static void GetOddNumber(object obj)
        {
            lock (OddList)
            {
                for (int i = 0; i <= 10; i++)
                {
                    if (i % 2 != 0)
                    {
                        OddList.Add(i);
                    }
                }
                for (int i = 9; i >= 0; i--)
                {
                    if (i % 2 != 0)
                    {
                        OddList.Add(i);
                    }
                }
            }
        } 
        private static void GetEvenNumber(object obj)
        {
            lock (EvenList)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (i % 2 == 0)
                    {
                        EvenList.Add(i);
                    }
                }
                for (int i = 9; i >= 0; i--)
                {
                    if (i % 2 == 0)
                    {
                        EvenList.Add(i);
                    }
                }
            }
        }
    }
}
