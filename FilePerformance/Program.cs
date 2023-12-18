using BenchmarkDotNet.Running;
using FilePerformance;

class MainClass
{
    public static void Main()
    {
        //BenchmarkRunner.Run<FileRead>();

        //new FileCreate().CreateFileWithStreamWriter();
        //new FileRead().ReadFileWithBuffer();

        MostrarListagem();
    }

    private static void MostrarListagem()
    {
        var lista = new FileRead().ReadFileWithBuffer();
        var count = 0;

        foreach (var file in lista)
        {
            count++;
            //Console.WriteLine($"Contador: {count} - Nome: {file}");
        }
    }
}
