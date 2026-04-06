using ChatR.Models;
using ChatR.Models.Constatns;
using Microsoft.EntityFrameworkCore;

namespace ChatR.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserInRoom> UsersInRoom { get; set; }
    public DbSet<Room> Rooms { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AddMessages(modelBuilder);
        AddUsers(modelBuilder);
        AddRooms(modelBuilder);
        AddUsersInRoom(modelBuilder);
    }

    private static void AddMessages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(message =>
        {
            message.ToTable("messages");

            message
                .Property(p => p.Content)
                .IsRequired();
            message
                .Property(p => p.Timestamp)
                .HasDefaultValueSql(Sql.NOW)
                .IsRequired();

            message
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void AddUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("users");

            user.HasIndex(x => x.Email).IsUnique();

            user
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql(Sql.NOW)
                .IsRequired();
            user
                .Property(x => x.LastLogin);
            user
                .Property(p => p.Email)
                .IsRequired();
            user
                .Property(p => p.Password)
                .IsRequired();
            user
                .Property(p => p.FirstName)
                .IsRequired();
            user
                .Property(p => p.LastName)
                .IsRequired();
            user
                .Property(p => p.Patronymic);
            user
                .Property(p => p.Role)
                .HasDefaultValue(UserRole.User)
                .IsRequired();
        });
    }

    private static void AddRooms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(room =>
        {
            room.ToTable("rooms");

            room
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql(Sql.NOW)
                .IsRequired();
            room
                .Property(x => x.IsClosed)
                .HasDefaultValue(false)
                .IsRequired();
            room
                .Property(x => x.ClosedAt);
            room
                .Property(p => p.Name)
                .IsRequired();
        });
    }

    private static void AddUsersInRoom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserInRoom>(usersInRoom =>
        {
            usersInRoom.ToTable("userInRoom");

            usersInRoom.HasIndex(x => new { x.UserId, x.RoomId }).IsUnique();

            usersInRoom
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql(Sql.NOW)
                .IsRequired();
            usersInRoom
                .Property(p => p.RoomRole)
                .HasDefaultValue(RoomRole.Member)
                .IsRequired();

            usersInRoom
                .HasOne(p => p.User)
                .WithMany(p => p.UsersInRoom)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            usersInRoom
                .HasOne(p => p.Room)
                .WithMany(p => p.UsersInRoom)
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
