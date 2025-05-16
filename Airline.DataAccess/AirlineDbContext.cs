using Microsoft.EntityFrameworkCore;

public class AirlineDbContext : DbContext
{
    public AirlineDbContext(DbContextOptions<AirlineDbContext> options) : base(options) { }

    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Seat> Seats => Set<Seat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Passenger)
            .WithOne(p => p.Seat)
            .HasForeignKey<Seat>(s => s.PassengerId);

        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Flight)
            .WithMany(f => f.Seats)
            .HasForeignKey(s => s.FlightId);

        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Flight)
            .WithMany(f => f.Passengers)
            .HasForeignKey(p => p.FlightId);
        modelBuilder.Entity<Seat>()
            .Property(s => s.RowVersion)
            .IsRowVersion();
    }
}
