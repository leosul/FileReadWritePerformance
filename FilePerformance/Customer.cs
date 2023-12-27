using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace FilePerformance;

[Delimiter(",")]
public class Customer
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Street { get; set; }

    public string ZipCode { get; set; }

    public string DoorNumber { get; set; }

    public string City { get; set; }

    public string Parish { get; set; }

    public string Country { get; set; }

    public DateTime ClientSince { get; set; }

    public bool IsActive { get; set; }

    public Customer()
    {
        
    }
    public Customer(
        string firstName, string lastName,
        string street, string zipCode,
        string doorNumber, string city,
        string parish, string country,
        DateTime clientSince, bool isActive)
    {
        FirstName = firstName;
        LastName = lastName;
        Street = street;
        ZipCode = zipCode;
        DoorNumber = doorNumber;
        City = city;
        Parish = parish;
        Country = country;
        ClientSince = clientSince;
        IsActive = isActive;
    }
}

public sealed class CustomerMap : ClassMap<Customer>
{
    public CustomerMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(s => s.FirstName).Index(0);
        Map(s => s.LastName).Index(1);
        Map(s => s.Street).Index(2);
        Map(s => s.ZipCode).Index(3);
        Map(s => s.DoorNumber).Index(4);
        Map(s => s.City).Index(5);
        Map(s => s.Parish).Index(6);
        Map(s => s.Country).Index(7);
        Map(s => s.ClientSince).Ignore();
        Map(s => s.IsActive).Ignore();
    }
}
