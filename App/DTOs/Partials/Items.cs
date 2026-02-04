namespace App.DTOs;

public record Items
{
    public required int PerPage { get; set; }
    public required int Count { get; set; }
    public required int Total { get; set; }
}