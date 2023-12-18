namespace FilePerformance;

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
