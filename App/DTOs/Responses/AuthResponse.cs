namespace App.DTOs;

public record AuthResponse
{
    public int UserId { get; set; }
    public required string Username { get; set; }
}