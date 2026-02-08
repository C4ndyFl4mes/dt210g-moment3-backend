using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class PostModel
{
    public int Id { get; set; }

    [Required, MinLength(3), MaxLength(200)]
    public required string Title { get; set; }

    [Required, MinLength(3)]
    public required string Content { get; set; }

    public required DateTime CreatedAt { get; set; }

    // Posted by User.
    public required UserModel Author { get; set; }
    public required int AuthorId { get; set; }
}