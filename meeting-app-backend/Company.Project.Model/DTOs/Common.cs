namespace Company.Project.Model.DTOs;

public record ApiResponse<T>(bool Success, string Message, T? Data);
