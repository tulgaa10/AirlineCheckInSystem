public class PassengerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public string? FlightNumber { get; set; }
    public string? SeatNumber { get; set; }
}
