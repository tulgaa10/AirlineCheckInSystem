using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// FlightsController angilal ni nislegiin medeeleltei holbootoi API endpoint-uudiig zohistsuuldag.
/// Nislegiin jagsaalt harah, nisleg haih, check-in hiih bolon nislegiin tuluviin oorchilolt hiih
/// uildluudiig udirddag controller yuм.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly AirlineDbContext _context;
    private readonly IHubContext<FlightHub> _hubContext;

    /// <summary>
    /// FlightsController-iin constructor.
    /// Ogogdliin sangiin context bolon SignalR-iin hub context-iig gadnaas injection hiij avna.
    /// </summary>
    /// <param name="context">Ogogdliin sangiin context</param>
    /// <param name="hubContext">SignalR-iin hub context (real-time medeelel damjuulah)</param>
    public FlightsController(AirlineDbContext context, IHubContext<FlightHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Buh nislegiin medeelliig avah API endpoint.
    /// GET /api/flights
    /// </summary>
    /// <returns>Buh nislegiin jagsaalt, suudluud bolon zorchigchdiin holbootoi medeeleltei</returns>
    [HttpGet]
    public async Task<IActionResult> GetFlights()
    {
        // Buh nisleg, tedgeeriin suudal bolon zorchigchdiin medeelliig tataj avah
        var flights = await _context.Flights
            .Include(f => f.Seats)
            .Include(f => f.Passengers)
            .ToListAsync();

        return Ok(flights);
    }

    /// <summary>
    /// ID-gaar nisleg haih API endpoint.
    /// GET /api/flights/{id}
    /// </summary>
    /// <param name="id">Haih nislegiin ID dugaar</param>
    /// <returns>Nislegiin buren medeelel esvel NotFound hariu</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Flight>> GetFlight(int id)
    {
        // ID-gaar nislegiig suudaltai ni haij oloh
        var flight = await _context.Flights
            .Include(f => f.Seats)  // Suudliin medeelliig orulj avna
            .AsNoTracking()  // Unshih uildeld gurven ashiglahgui tul tracking hiih shaardlagagui
            .FirstOrDefaultAsync(f => f.Id == id);

        // Herev nisleg oldoogui bol 404 NotFound hariu butsaana
        if (flight == null)
            return NotFound();

        // Oldson bol nislegiin medeelliig butsaana
        return Ok(flight);
    }

    /// <summary>
    /// Debugging zoriulalttai nislegiin suudliin medeelel gargah API endpoint.
    /// GET /api/flights/debug/{id}
    /// </summary>
    /// <param name="id">Haih nislegiin ID dugaar</param>
    /// <returns>Nislegiin bolon suudluudiin debuglah medeelel</returns>
    [HttpGet("debug/{id}")]
    public async Task<IActionResult> DebugFlightSeats(int id)
    {
        // ID-gaar nislegiig suudaltai ni haij oloh
        var flight = await _context.Flights
            .Include(f => f.Seats)
            .FirstOrDefaultAsync(f => f.Id == id);

        // Herev nisleg oldoogui bol 404 NotFound hariu butsaana
        if (flight == null)
            return NotFound("Flight not found");

        // Nisleg bolon suudluudiin talaar degelrengui medeelel butsaah
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

    /// <summary>
    /// Zorchigchiin check-in hiih API endpoint.
    /// Zorchigchiig nisleg deer burtgej, songogdson suudliig onoodog.
    /// POST /api/flights/checkin
    /// </summary>
    /// <param name="dto">Check-in hiih medeelliig aguulsan DTO</param>
    /// <returns>Check-in amjilttai bolson esvel aldaanii shinj</returns>
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckInPassenger([FromBody] PassengerCheckInDto dto)
    {
        // DTO-n medeelel zov esehiig shalgah
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Passport bolon nislegiin ID-gaar zorchigch haih
        var passenger = await _context.Passengers
            .FirstOrDefaultAsync(p => p.PassportNumber == dto.PassportNumber && p.FlightId == dto.FlightId);

        // Herev zorchigch oldoogui bol aldaanii medeegdel butsaah
        if (passenger == null)
            return NotFound(new ProblemDetails { Title = "Passenger not found", Detail = "No matching passenger for this flight." });

        // Suudliin dugaar bolon nislegiin ID-gaar suudal haih
        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == dto.FlightId && s.SeatNumber == dto.SeatNumber);

        // Herev suudal oldoogui bol aldaanii medeegdel butsaah
        if (seat == null)
            return BadRequest(new ProblemDetails { Title = "Invalid Seat", Detail = "Seat does not exist." });

        // Herev suudal avagdsan baival aldaanii medeegdel butsaah
        if (seat.IsAssigned)
            return BadRequest(new ProblemDetails { Title = "Seat Unavailable", Detail = "Seat is already assigned to another passenger." });

        try
        {
            // Suudliig zorchigchid onooh
            seat.PassengerId = passenger.Id;
            seat.IsAssigned = true;

            // Ogogdliin sand oorchiloltuudiig hadgalah
            await _context.SaveChangesAsync();
            return Ok("Check-in successful");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrency-tei holbootoi aldaa garval conflict aldaag butsaah
            return Conflict(new ProblemDetails { Title = "Concurrency Conflict", Detail = "Seat was just taken by someone else." });
        }
    }

    /// <summary>
    /// Nislegiin tuluviig oorchiloh API endpoint.
    /// Nislegiin tuluviig oorchilj, buh client ruu SignalR ashiglan medeelliig broadcast hiine.
    /// POST /api/flights/update-status
    /// </summary>
    /// <param name="dto">Nislegiin tuluv oorchiloh medeelliig aguulsan DTO</param>
    /// <returns>Amjilttai bolson esvel aldaanii shinj</returns>
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateFlightStatus([FromBody] FlightStatusChangeRequestDto dto)
    {
        // DTO-n medeelel zov esehiig shalgah
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Nislegiin ID-gaar nisleg haih
        var flight = await _context.Flights.FindAsync(dto.FlightId);

        // Herev nisleg oldoogui bol aldaanii medeegdel butsaah
        if (flight == null)
            return NotFound(new ProblemDetails { Title = "Flight Not Found", Detail = $"No flight with ID {dto.FlightId}" });

        // Nislegiin tuluviig oorchiloh
        flight.Status = dto.NewStatus;

        // Ogogdliin sand oorchiloltuudiig hadgalah
        await _context.SaveChangesAsync();

        // Zovhon oorchlogdson nislegiin medeelliig broadcast hiih
        var update = new FlightStatusUpdateDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            Status = flight.Status.ToString()
        };

        // SignalR oor damjuulan buh client ruu medeelel ilgeeh
        await _hubContext.Clients.All.SendAsync("ReceiveFlightUpdate", update);

        return Ok("Flight status updated and broadcasted");
    }
}