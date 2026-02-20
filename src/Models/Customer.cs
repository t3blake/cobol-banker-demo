namespace CobolBanker.Models;

public class Customer
{
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
