namespace App.DTOs;

public record GuardResponse
{
    public bool IsAdmin { get; set; } = false;
}