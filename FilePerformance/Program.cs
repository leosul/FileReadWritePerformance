using BenchmarkDotNet.Running;
using FilePerformance;

class MainClass
{
    public static void Main()
    {
        //BenchmarkRunner.Run<FileRead>();

        new FileRead().ReadFileWithBuffer();
    }
}
