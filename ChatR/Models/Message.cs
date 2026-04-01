namespace ChatR.Models
{
    public record Message
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public required int RoomId { get; set; }
        public virtual Room? Room { get; set; }
    }
}
