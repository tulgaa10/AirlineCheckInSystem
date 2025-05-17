using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class AirlineDbContext : DbContext
{
    public AirlineDbContext(DbContextOptions<AirlineDbContext> options)
        : base(options)
    {


    }



    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Seat> Seats => Set<Seat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Flight -> Passengers (One-to-Many)
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Flight)
            .WithMany(f => f.Passengers)
            .HasForeignKey(p => p.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        // Flight -> Seats (One-to-Many)
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Flight)
            .WithMany(f => f.Seats)
            .HasForeignKey(s => s.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        // Passenger -> Seat (Optional One-to-One)
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Passenger)
            .WithOne(p => p.Seat)
            .HasForeignKey<Seat>(s => s.PassengerId)
            .OnDelete(DeleteBehavior.SetNull); // so we don’t delete passenger or seat accidentally

        // RowVersion for concurrency check
        modelBuilder.Entity<Seat>()
            .Property(s => s.RowVersion)
            .IsRowVersion();

        // Optional: Unique constraint for flight number
        modelBuilder.Entity<Flight>()
            .HasIndex(f => f.FlightNumber)
            .IsUnique();
    }
}
