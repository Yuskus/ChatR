namespace ChatR.Models
{
    public class UserInRoom
    {
        public int Id { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.Now;

        public required RoomRole RoomRole { get; set; } = RoomRole.Member;
        public required int UserId { get; set; }
        public virtual User? User { get; set; }
        public required int RoomId { get; set; }
        public virtual Room? Room { get; set; }
    }

    public enum RoomRole
    {
        Member,
        Editor,
        Admin
    }
}
