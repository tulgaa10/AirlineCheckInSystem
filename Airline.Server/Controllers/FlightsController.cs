using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly AirlineDbContext _context;
    private readonly IHubContext<FlightHub> _hubContext;


    public FlightsController(AirlineDbContext context, IHubContext<FlightHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;

    }

    [HttpGet]
    public async Task<IActionResult> GetFlights()
    {
        var flights = await _context.Flights
            .Include(f => f.Seats)
            .Include(f => f.Passengers)
            .ToListAsync();

        return Ok(flights);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlight(int id)
    {
        var flight = await _context.Flights
            .Include(f => f.Seats)
            .Include(f => f.Passengers)
            .FirstOrDefaultAsync(f => f.Id == id);

        return flight == null ? NotFound() : Ok(flight);
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckInPassenger(PassengerCheckInDto dto)
    {
        var passenger = await _context.Passengers
            .FirstOrDefaultAsync(p => p.PassportNumber == dto.PassportNumber && p.FlightId == dto.FlightId);

        if (passenger == null)
            return NotFound("Passenger not found");

        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == dto.FlightId && s.SeatNumber == dto.SeatNumber);

        if (seat == null || seat.IsAssigned)
            return BadRequest("Seat not available");

        // Assign seat
        seat.PassengerId = passenger.Id;
        seat.IsAssigned = true;
        await _context.SaveChangesAsync();

        return Ok("Check-in successful");
    }
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateFlightStatus([FromBody] FlightStatusChangeRequestDto dto)
    {
        var flight = await _context.Flights.FindAsync(dto.FlightId);
        if (flight == null) return NotFound("Flight not found");
        flight.Status = dto.NewStatus;
        await _context.SaveChangesAsync();

        // Broadcast updated statuses
        var updates = await _context.Flights.Select(f => new FlightStatusUpdateDto
        {
            FlightId = f.Id,
            FlightNumber = f.FlightNumber,
            Status = f.Status.ToString()
        }).ToListAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveFlightUpdates", updates);
        return Ok("Flight status updated and broadcasted");
    }


}
