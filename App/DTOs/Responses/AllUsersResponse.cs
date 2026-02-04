namespace App.DTOs;

public record AllUsersResponse
{
    public List<AuthResponse> Users { get; set; } = [];
}