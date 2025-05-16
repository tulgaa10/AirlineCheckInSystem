using System.ComponentModel.DataAnnotations;

public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public bool IsAssigned { get; set; } = false;
    public int FlightId { get; set; }
    public Flight Flight { get; set; }
    public int? PassengerId { get; set; }
    public Passenger? Passenger { get; set; }

    [Timestamp] // <-- This enables concurrency protection
    public byte[]? RowVersion { get; set; }
}
