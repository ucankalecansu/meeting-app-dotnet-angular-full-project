namespace Company.Project.Model.DTOs;

public record UserRegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password);

public record UserLoginRequest(
    string Email,
    string Password);

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? ProfileImagePath,
    string Token);

public record UploadProfileImageRequest(
    Guid UserId,
    string FileName,
    string Base64Image);
