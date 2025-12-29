using System;
using System.Threading;

namespace TestConsoleApp
{
    public class Program
    {
        // args[0] = iterations
        // args[1] = exit code
        public static int Main(string[] args)
        {
            int count = 0;
            int max = args?.Length > 0 ? int.Parse(args[0]) : 0;
            while (count < max)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Iteration " + count);
                count++;
            }
            Console.WriteLine("Finished");
            if (args?.Length > 1)
            {
                return int.Parse(args[1]);
            }
            return 0;
        }
    }
}
