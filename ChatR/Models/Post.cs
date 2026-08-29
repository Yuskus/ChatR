namespace ChatR.Models;

public record Post
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public required string Content { get; set; }

    public int UserId { get; set; }
    public virtual User? User { get; set; }
}