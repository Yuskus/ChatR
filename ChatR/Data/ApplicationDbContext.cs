namespace ChatR.Data;

using ChatR.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(m =>
        {
            m.ToTable("messages");

            m.Property(p => p.User).IsRequired();
            m.Property(p => p.Content).IsRequired();
            m.Property(p => p.Timestamp).IsRequired();
        });
    }

    public DbSet<Message> Messages { get; set; }
}
