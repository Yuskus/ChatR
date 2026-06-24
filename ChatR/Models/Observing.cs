namespace ChatR.Models;

public record Observing
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int UserFromId { get; set; }
    public virtual User? UserFrom { get; set; }
    public int UserToId { get; set; }
    public virtual User? UserTo { get; set; }
}