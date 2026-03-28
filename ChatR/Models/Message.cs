namespace ChatR.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
