namespace Company.Project.Model.Entities;

public class MeetingDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MeetingId { get; set; }
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public Meeting Meeting { get; set; } = default!;
}
