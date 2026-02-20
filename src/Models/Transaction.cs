namespace CobolBanker.Models;

public class Transaction
{
    public long TransactionId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
}
