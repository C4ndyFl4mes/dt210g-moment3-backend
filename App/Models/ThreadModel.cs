using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class ThreadModel
{
    public int Id { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public required string Title { get; set; }

    [Required]
    public required DateTime Published { get; set; }

    [Required, MinLength(3), MaxLength(400)]
    public required string InitialMessage { get; set; }

    // Thread owned by User.
    public required UserModel CreatedBy { get; set; }

    // Posts in Thread.
    public List<PostModel>? Posts { get; set; }
}