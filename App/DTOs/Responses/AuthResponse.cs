namespace App.DTOs;

public record AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}