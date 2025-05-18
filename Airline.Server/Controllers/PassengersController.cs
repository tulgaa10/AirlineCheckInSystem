using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airline.DataAccess;

/// <summary>
/// PassengersController angilal ni aylagcin medeeleltei holbootoi API endpoint-uudiig zohitsuuldag.
/// aylagcin hailt hiih bolon suudal onooh uildluudiig udirdana.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PassengersController : ControllerBase
{
    private readonly AirlineDbContext _context;

    /// <summary>
    /// PassengersController-iin constructor.
    /// Ogogdliin sangiin context-iig gadnaas injection hiij avna.
    /// </summary>
    /// <param name="context">Ogogdliin sangiin context</param>
    public PassengersController(AirlineDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Passport dugaaraar aylagch haih API endpoint.
    /// Ene API ni aylagcin medeelliig passport dugaaraar haij oldog.
    /// GET /api/passengers/search?passportNumber={dugaar}
    /// </summary>
    /// <param name="passportNumber">Haih niislegchiin passport dugaar</param>
    /// <returns>Niislegchiin buren medeelel esvel NotFound hariu</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchByPassport([FromQuery] string passportNumber)
    {
        // Passport dugaaraar niislegchiin medeelliig oloh
        // Mun tuunii suudal bolon niislegiin holbootoi medeelliig tataj avna
        var passenger = await _context.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.PassportNumber == passportNumber);

        // Herev oldoogui bol 404 NotFound hariu butsaana
        if (passenger == null) return NotFound();

        // Oldson bol niislegchiin medeelliig butsaana
        return Ok(passenger);
    }

    /// <summary>
    /// Niislegchid suudal onooh API endpoint.
    /// Ene API ni tuhain niislegchid songogdson suudliig onooj ogdog.
    /// POST /api/passengers/{passengerId}/assign-seat
    /// </summary>
    /// <param name="passengerId">Niislegchiin ID dugaar</param>
    /// <param name="seatNumber">Onooh suudliin dugaar</param>
    /// <returns>Onoogdson suudliin medeelel esvel aldaanii shinj</returns>
    [HttpPost("{passengerId}/assign-seat")]
    public async Task<IActionResult> AssignSeat(int passengerId, [FromBody] string seatNumber)
    {
        // Niislegchiin ID-gaar niislegchiig haij oloh
        var passenger = await _context.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.Id == passengerId);

        // Herev niislegch oldoogui bol aldaanii hariu butsaana
        if (passenger == null) return NotFound("Passenger not found");

        // Niislegchiin niislegiin ID-g avah
        var flightId = passenger.FlightId;

        // Suudal niisleg deer bgaa esehiig shalgah
        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);

        // Herev suudal oldoogui bol aldaanii hariu butsaana
        if (seat == null) return BadRequest("Seat does not exist");

        // Herev suudal uur hunii nertei baival aldaanii hariu butsaana
        if (seat.IsAssigned) return BadRequest("Seat is already taken");

        // Herev niislegch uur suudaltai bol aldaanii hariu butsaana
        if (passenger.Seat != null)
        {
            return BadRequest("Passenger already has a seat assigned");
        }

        // Suudal onooh
        seat.IsAssigned = true;
        seat.PassengerId = passengerId;
        passenger.Seat = seat;

        // Ogogdliin sand hadgalah
        await _context.SaveChangesAsync();

        // TODO: daraa ni SignalR ashiglan client-uudad medegdeh

        // Amjilttai bolson tul suudliin medeelliig butsaana
        return Ok(seat);
    }
}