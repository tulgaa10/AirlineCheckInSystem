public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public FlightStatus Status { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}

public enum FlightStatus
{
    Registering,
    Boarding,
    Departed,
    Delayed,
    Cancelled
}
