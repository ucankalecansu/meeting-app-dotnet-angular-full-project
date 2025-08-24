namespace Company.Project.Model.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Display name helper property
    public string FullName => $"{FirstName} {LastName}";
}
