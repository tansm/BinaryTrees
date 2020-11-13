
using System;
using System.Diagnostics;

class Program
{
    public static void Main(string[] args)
    {
        //Run(BinaryTreesV1.Run, args);
        
        Run(BinaryTreesV2.Run, args);
        //Run(BinaryTreesV3.Run, args);
        //Run(BinaryTreesV4.Run, args);

        Console.ReadLine();
    }

    public static void Run(Action<string[]> action, string[] args)
    {
        var s = Stopwatch.StartNew();
        action(args);
        s.Stop();
        Console.WriteLine("Time = " + s.Elapsed);
    }
}