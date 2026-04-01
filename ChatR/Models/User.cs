namespace ChatR.Models
{
    public record User
    {
        public int Id { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.Now;

        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Patronymic { get; set; }
        public required UserRole Role { get; set; } = UserRole.User;
        public virtual List<UserInRoom> UsersInRoom { get; set; } = [];
    }

    public enum UserRole
    {
        User,
        Editor,
        Admin
    }
}
