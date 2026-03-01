using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Appointment>()
            .HasIndex(x => new { x.StartAt, x.Status });

        b.Entity<Appointment>()
            .Property(x => x.ServiceName).HasMaxLength(120);

        b.Entity<Appointment>()
            .Property(x => x.DecisionToken).HasMaxLength(120);
    }
}