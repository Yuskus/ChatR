namespace ChatR.Models
{
    public record Room
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsClosed { get; set; } = false;
        public DateTime? ClosedAt { get; set; }
        public DateTime? LastMessage { get; set; }
        public required string Name { get; set; }
        public virtual List<UserInRoom> UsersInRoom { get; set; } = [];
    }
}
