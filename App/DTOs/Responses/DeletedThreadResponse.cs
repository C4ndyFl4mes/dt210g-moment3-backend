namespace App.DTOs;


public record DeletedThreadResponse
{
    public required int ThreadId { get; set; }
}