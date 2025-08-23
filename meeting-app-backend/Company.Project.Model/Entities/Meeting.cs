using System.ComponentModel.DataAnnotations.Schema;

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
    public string Status { get; set; } = "active"; // active, cancelled
    
    // Katılımcı e-postaları virgülle ayrılmış şekilde saklanacak
    public string? ParticipantEmails { get; set; }
    
    [NotMapped]
    public IEnumerable<string> Participants
    {
        get => string.IsNullOrEmpty(ParticipantEmails) 
            ? Array.Empty<string>() 
            : ParticipantEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
        set => ParticipantEmails = value == null ? null : string.Join(',', value);
    }

    public ICollection<MeetingDocument> Documents { get; set; } = new List<MeetingDocument>();
}
