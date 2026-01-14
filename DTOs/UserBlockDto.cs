namespace Diversion.DTOs;

public class CreateUserBlockDto
{
    public string BlockedUserId { get; set; }
    public string? Reason { get; set; }
}

public class UserBlockDto
{
    public int Id { get; set; }
    public string BlockedUserId { get; set; }
    public string BlockedUsername { get; set; }
    public string? BlockedDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
