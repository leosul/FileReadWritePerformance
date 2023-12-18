using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace FilePerformance;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FileRead
{
    [Benchmark]
    public void ReadFileWithStreamReader()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        using StreamReader reader = new(@"C:\dev\temp\temp\FileTest.csv");
        reader.ReadLine();

        var count = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Split(',');

            //do something...
            count++;
        }

        sw.Stop();
        ConsoleLog("ReadFileWithStreamReader", sw, count);
    }

    [Benchmark]
    public void ReadFileWithParallelism()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        const int bufferSize = 4096;
        var count = 0;

        using (StreamReader reader = new(@"C:\dev\temp\temp\FileTest.csv"))
        {
            reader.ReadLine();

            Parallel.ForEach(ReadLinesInChunks(reader, bufferSize), chunk =>
            {
                var chunkLines = chunk.Split('\n');

                //do something
                count++;
            });
        }

        sw.Stop();
        ConsoleLog("ReadFileWithParallelism", sw, count);
    }

    [Benchmark]
    public void ReadFileWithReadAllLines()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var filePath = @"C:\dev\temp\temp\FileTest.csv";
        var lines = File.ReadAllLines(filePath);
        var count = 0;

        foreach (var line in lines)
        {
            var lineAux = line.Split(',');

            //do something

            count++;
        }

        sw.Stop();
        ConsoleLog("ReadFileWithReadAllLines", sw, count);
    }

    [Benchmark]
    public void ReadFileWithOpenRead()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var filePath = @"C:\dev\temp\temp\FileTest.csv";
        string line;
        var count = 0;

        using var fs = File.OpenRead(filePath);
        using var reader = new StreamReader(fs);
        reader.ReadLine();

        while ((line = reader.ReadLine()) != null)
        {
            var lineAux = line.Split(',');

            //do something
            count++;
        }

        sw.Stop();
        ConsoleLog("ReadFileWithOpenRead", sw, count);
    }

    [Benchmark]
    public void ReadFileWithSpan()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var filePath = @"C:\dev\temp\temp\FileTest.csv";
        string line;
        var count = 0;

        using var fs = File.OpenRead(filePath);
        using var reader = new StreamReader(fs);
        reader.ReadLine();

        while ((line = reader.ReadLine()) != null)
        {
            var span = line.AsSpan();
            var firstCommaIndex = span.IndexOf(',');
            var column1 = span[..firstCommaIndex].ToString();

            span = span[(firstCommaIndex + 1)..].ToString();
            firstCommaIndex = span.IndexOf(',');
            var column2 = span[..firstCommaIndex].ToString();

            //do something

            count++;
        }

        sw.Stop();
        ConsoleLog("ReadFileWithSpan", sw, count);
    }

    [Benchmark]
    public IEnumerable<string> ReadFileWithBuffer()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var filePath = @"C:\dev\temp\temp\FileTest.csv";
        var rawBuffer = new byte[1024 * 1024];
        var count = 0;

        using (var fs = File.OpenRead(filePath))
        {
            var bytesBuffered = 0;
            var bytesConsumed = 0;
            var isFirstLine = true;

            while (true)
            {
                var bytesRead = fs.Read(rawBuffer, bytesBuffered, rawBuffer.Length - bytesBuffered);

                if (bytesRead == 0) break;

                bytesBuffered += bytesRead;

                int linePosition;

                do
                {
                    linePosition = Array.IndexOf(rawBuffer, (byte)'\n', bytesConsumed, bytesBuffered - bytesConsumed);

                    if (linePosition >= 0)
                    {
                        var lineLength = linePosition - bytesConsumed;
                        var line = new Span<byte>(rawBuffer, bytesConsumed, lineLength);
                        bytesConsumed += lineLength + 1;

                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue;
                        }

                        var span = line[(line.IndexOf((byte)',') + 1)..];
                        var firstCommaIndex = span.IndexOf((byte)',');
                        var column1 = Encoding.UTF8.GetString(line[..(firstCommaIndex + 1)]);

                        span = line[(line.IndexOf((byte)',') + 1)..];
                        firstCommaIndex = span.IndexOf((byte)',');
                        var column2 = Encoding.UTF8.GetString(span[..firstCommaIndex]);

                        //do something

                        yield return column1;

                        count++;
                    }
                } while (linePosition >= 0);

                Array.Copy(rawBuffer, bytesConsumed, rawBuffer, 0, bytesBuffered - bytesConsumed);
                bytesBuffered -= bytesConsumed;
                bytesConsumed = 0;
            }
        }

        sw.Stop();
        ConsoleLog("ReadFileWithBuffer", sw, count);
    }

    private static IEnumerable<string> ReadLinesInChunks(StreamReader reader, int bufferSize)
    {
        char[] buffer = new char[bufferSize];
        int bytesRead;
        string remaining = string.Empty;

        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            var chunk = new string(buffer, 0, bytesRead);
            var lines = (remaining + chunk).Split('\n');

            for (int i = 0; i < lines.Length - 1; i++)
            {
                yield return lines[i];
            }

            remaining = lines[^1];
        }

        if (!string.IsNullOrEmpty(remaining))
        {
            yield return remaining;
        }
    }
    private static void ConsoleLog(string method, Stopwatch sw, int count)
    {
        long memoryUsed = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

        var line1 = $"| Elapsed time for file reading: {Math.Round(sw.Elapsed.TotalSeconds, 2)} seconds";
        var line2 = $"| Memory usage: {Math.Round((double)memoryUsed, 2)} MB";
        var line3 = $"| Total records read {count:#,##0}";

        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);

        var title = $"| Results {method}";

        Console.WriteLine("+-------------------------------------------------------------------------------------+");
        Console.WriteLine($"{title,-86}|");
        Console.WriteLine("+-------------------------------------------------------------------------------------+");
        Console.WriteLine($"|                                                                                     |");
        Console.WriteLine($"{line1,-86}|");
        Console.WriteLine($"{line2,-86}|");
        Console.WriteLine($"{line3,-86}|");
        Console.WriteLine($"|                                                                                     |");
        Console.WriteLine($"| Gen0: {gen0,-78}|");
        Console.WriteLine($"| Gen1: {gen1,-78}|");
        Console.WriteLine($"| Gen2: {gen2,-78}|");
        Console.WriteLine($"|                                                                                     |");
        Console.WriteLine("+-------------------------------------------------------------------------------------+");
    }
}