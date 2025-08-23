namespace Company.Project.Model.DTOs;

public record MeetingCreateRequest(
    string Title, 
    DateTime StartAt, 
    DateTime EndAt, 
    string? Description,
    IEnumerable<string>? Participants,
    string Status = "active"
);

public record MeetingUpdateRequest(
    string Title, 
    DateTime StartAt, 
    DateTime EndAt, 
    string? Description,
    IEnumerable<string>? Participants,
    string Status = "active"
);

public record MeetingCancelRequest(string? CancellationReason);

public record MeetingDetailsResponse(
    Guid Id, 
    string Title, 
    DateTime StartAt, 
    DateTime EndAt,
    string? Description, 
    bool IsCancelled, 
    DateTime? CancelledAt, 
    string Status,
    IEnumerable<string> Participants,
    List<MeetingDocumentDto> Documents
);

public record MeetingDocumentDto(Guid Id, string FileName, string FilePath, DateTime UploadedAt);
