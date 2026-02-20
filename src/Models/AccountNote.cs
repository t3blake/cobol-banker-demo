namespace CobolBanker.Models;

public class AccountNote
{
    public long NoteId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string NoteText { get; set; } = string.Empty;
}
