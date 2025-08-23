namespace Company.Project.Model.Entities;

public class DeletedMeetingLog
{
    public int Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Title { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Description { get; set; }
    public bool WasCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime DeletedAt { get; set; }
    public string DeletedBy { get; set; }
}
