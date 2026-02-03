namespace App.Models;

public class ThreadModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required DateTime Published { get; set; }
    public required string InitialMessage { get; set; }

    // Thread owned by User.
    public required UserModel CreatedBy { get; set; }

    // Posts in Thread.
    public List<PostModel>? Posts { get; set; }
}