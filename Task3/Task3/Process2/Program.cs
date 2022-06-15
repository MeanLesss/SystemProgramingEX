using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Process2
{
    internal class Program
    {
        public static List<int> EvenNumber = new List<int>();

        static void Main(string[] args)
        {
            Thread[] threads = new Thread[1];
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i] = new Thread(UpdateFields);
                threads[i].Start();
            }

            for (int i = 0; i < threads.Length; ++i)
                threads[i].Join();

            Console.WriteLine("Process 2:");
            foreach (var Even in EvenNumber)
            {
                Console.WriteLine(Even);
            }
            Console.Read();

        }
        int count;

        public static void UpdateFields()
        {
            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 == 0)
                {
                    EvenNumber.Add(i);
                }
            }

            for (int i = 9; i > 0; i--)
            {
                if (i % 2 == 0)
                {
                    EvenNumber.Add(i);
                }
            }
        }
    }
}
