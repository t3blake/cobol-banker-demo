namespace CobolBanker.Models;

public class Account
{
    public string AccountNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Checking, Savings, Money Market
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active"; // Active, Frozen, Closed
    public string OpenedDate { get; set; } = string.Empty;
}
