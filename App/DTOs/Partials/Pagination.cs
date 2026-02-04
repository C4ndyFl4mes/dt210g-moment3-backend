namespace App.DTOs;

public record Pagination
{
    public required int LastPage { get; set; }
    public required int CurrentPage { get; set; }
    public required Items Items { get; set; }
}