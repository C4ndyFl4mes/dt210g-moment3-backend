namespace App.DTOs;

public record NewThreadResponse
{
    public int ThreadId { get; set; }
    public bool Success { get; set; }
}