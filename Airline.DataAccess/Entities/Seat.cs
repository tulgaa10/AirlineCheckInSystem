using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Seat
{
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string SeatNumber { get; set; } = string.Empty;

    public bool IsAssigned { get; set; } = false;

    public int FlightId { get; set; }

    [JsonIgnore]
    public Flight Flight { get; set; } = default!;

    public int? PassengerId { get; set; }

    public Passenger? Passenger { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}