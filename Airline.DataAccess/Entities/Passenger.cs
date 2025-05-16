public class Passenger
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;

    public int FlightId { get; set; }
    public Flight Flight { get; set; }

    public Seat? Seat { get; set; }
}
