namespace Company.Project.Model.DTOs;

public record MeetingCreateRequest(string Title, DateTime StartAt, DateTime EndAt, string? Description);
public record MeetingUpdateRequest(string Title, DateTime StartAt, DateTime EndAt, string? Description);
public record MeetingCancelRequest(string? CancellationReason);
public record MeetingDetailsResponse(Guid Id, string Title, DateTime StartAt, DateTime EndAt, 
    string? Description, bool IsCancelled, DateTime? CancelledAt, List<MeetingDocumentDto> Documents);
public record MeetingDocumentDto(Guid Id, string FileName, string FilePath, DateTime UploadedAt);
