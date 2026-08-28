namespace LeaveTracker.Api.Models;

public class ApprovalNote
{
    public Guid Id { get; set; }
    public Guid LeaveRequestId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
