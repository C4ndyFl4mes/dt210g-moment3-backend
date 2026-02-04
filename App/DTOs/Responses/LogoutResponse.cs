namespace App.DTOs;

public record LogoutResponse
{
    public required bool IsLoggedIn { get; set; }
}