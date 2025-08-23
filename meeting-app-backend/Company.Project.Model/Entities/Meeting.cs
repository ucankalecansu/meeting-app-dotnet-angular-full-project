namespace Company.Project.Model.Entities;

public class Meeting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt   { get; set; }
    public string? Description { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    public ICollection<MeetingDocument> Documents { get; set; } = new List<MeetingDocument>();
}
