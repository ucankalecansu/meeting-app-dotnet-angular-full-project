using Company.Project.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Project.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<MeetingDocument> MeetingDocuments { get; set; }
    public DbSet<DeletedMeetingLog> DeletedMeetingLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser yapılandırması
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Meeting yapılandırması
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasMany(m => m.Documents)
                  .WithOne(d => d.Meeting)
                  .HasForeignKey(d => d.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MeetingDocument yapılandırması
        modelBuilder.Entity<MeetingDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
