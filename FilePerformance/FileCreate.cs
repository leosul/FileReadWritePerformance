using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Bogus;

namespace FilePerformance;

public class FileCreate
{
    public void CreateFileWithStreamWriter()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var customers = CreateCustomers();

        string filePath = @"C:\dev\temp\temp\FileTest.csv";

        using StreamWriter streamWriter = new(filePath);
        streamWriter.WriteLine("FirstName, LastName, Street, ZipCode, DoorNumber, City, Parish, Country, ClientSince, IsActive");

        foreach (var customer in customers)
        {
            var line = $"{customer.FirstName},{customer.LastName},{customer.Street},{customer.ZipCode}," +
                       $"{customer.DoorNumber},{customer.City},{customer.Parish},{customer.Country},{customer.ClientSince}," +
                       $"{customer.IsActive}";

            streamWriter.WriteLine(line);
        }

        sw.Stop();
        ConsoleLog("CreateFileWithStreamWriter", sw);
    }

    public void CreateFileWithParallelism()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();

        var customers = CreateCustomers().ToArray();

        string filePath = @"C:\dev\temp\temp\FileTest.csv";

        using StreamWriter streamWriter = new(filePath, false, Encoding.UTF8, 8192);
        streamWriter.WriteLine("FirstName, LastName, Street, ZipCode, DoorNumber, City, Parish, Country, ClientSince, IsActive");

        Parallel.ForEach(
            Partitioner.Create(0, customers.Length),
            range =>
            {
                StringBuilder sb = new();

                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Customer customer = customers[i];

                    var line = $"{customer.FirstName},{customer.LastName},{customer.Street},{customer.ZipCode}," +
                       $"{customer.DoorNumber},{customer.City},{customer.Parish},{customer.Country},{customer.ClientSince}," +
                       $"{customer.IsActive}";

                    sb.AppendLine(line);
                }

                lock (streamWriter)
                {
                    streamWriter.Write(sb.ToString());
                }
            });

        sw.Stop();
        ConsoleLog("CreateFileWithParallelism", sw);
    }

    private IEnumerable<Customer> CreateCustomers()
    {
        return new Faker<Customer>()
            .StrictMode(true)
            .RuleFor(s => s.FirstName, s => s.Name.FirstName())
            .RuleFor(s => s.LastName, s => s.Name.LastName())
            .RuleFor(s => s.Street, s => s.Address.StreetAddress(true))
            .RuleFor(s => s.ZipCode, s => s.Address.ZipCode())
            .RuleFor(s => s.DoorNumber, s => s.Address.BuildingNumber())
            .RuleFor(s => s.City, s => s.Address.City())
            .RuleFor(s => s.Parish, s => s.Address.County())
            .RuleFor(s => s.Country, s => s.Address.Country())
            .RuleFor(s => s.ClientSince, s => s.Date.Between(new DateTime(2000, 1, 1).Date, new DateTime(2023, 1, 1).Date))
            .RuleFor(s => s.IsActive, s => s.Random.Bool())
            .Generate(12_000_000);
    }

    private static void ConsoleLog(string method, Stopwatch sw)
    {
        long memoryUsed = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

        var line1 = $"| Elapsed time for file reading: {Math.Round(sw.Elapsed.TotalSeconds, 2)} seconds";
        var line2 = $"| Memory usage: {Math.Round((double)memoryUsed, 2)} MB";

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
        Console.WriteLine($"| Gen0: {gen0,-78}|");
        Console.WriteLine($"| Gen1: {gen1,-78}|");
        Console.WriteLine($"| Gen2: {gen2,-78}|");
        Console.WriteLine($"|                                                                                     |");
        Console.WriteLine("+-------------------------------------------------------------------------------------+");
    }
}
