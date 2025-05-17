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
    public async Task<ActionResult<Flight>> GetFlight(int id)
    {
        var flight = await _context.Flights
            .Include(f => f.Seats)  // This correctly includes the seats
            .AsNoTracking()  // Optional: improves performance for read-only operations
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flight == null)
            return NotFound();


        return Ok(flight);
    }
    [HttpGet("debug/{id}")]
    public async Task<IActionResult> DebugFlightSeats(int id)
    {
        var flight = await _context.Flights
            .Include(f => f.Seats)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flight == null)
            return NotFound("Flight not found");

        // Return detailed information about the flight and its seats
        var debugInfo = new
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            SeatsCount = flight.Seats.Count,
            Seats = flight.Seats.Select(s => new
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                IsAssigned = s.IsAssigned,
                PassengerId = s.PassengerId
            }).ToList()
        };

        return Ok(debugInfo);
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckInPassenger([FromBody] PassengerCheckInDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var passenger = await _context.Passengers
            .FirstOrDefaultAsync(p => p.PassportNumber == dto.PassportNumber && p.FlightId == dto.FlightId);

        if (passenger == null)
            return NotFound(new ProblemDetails { Title = "Passenger not found", Detail = "No matching passenger for this flight." });

        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == dto.FlightId && s.SeatNumber == dto.SeatNumber);

        if (seat == null)
            return BadRequest(new ProblemDetails { Title = "Invalid Seat", Detail = "Seat does not exist." });

        if (seat.IsAssigned)
            return BadRequest(new ProblemDetails { Title = "Seat Unavailable", Detail = "Seat is already assigned to another passenger." });

        try
        {
            seat.PassengerId = passenger.Id;
            seat.IsAssigned = true;

            await _context.SaveChangesAsync();
            return Ok("Check-in successful");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new ProblemDetails { Title = "Concurrency Conflict", Detail = "Seat was just taken by someone else." });
        }
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateFlightStatus([FromBody] FlightStatusChangeRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var flight = await _context.Flights.FindAsync(dto.FlightId);
        if (flight == null)
            return NotFound(new ProblemDetails { Title = "Flight Not Found", Detail = $"No flight with ID {dto.FlightId}" });

        flight.Status = dto.NewStatus;
        await _context.SaveChangesAsync();

        // Only broadcast the updated flight
        var update = new FlightStatusUpdateDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            Status = flight.Status.ToString()
        };

        await _hubContext.Clients.All.SendAsync("ReceiveFlightUpdate", update);

        return Ok("Flight status updated and broadcasted");
    }

}