using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airline.DataAccess;

[ApiController]
[Route("api/[controller]")]
public class PassengersController : ControllerBase
{
    private readonly AirlineDbContext _context;

    public PassengersController(AirlineDbContext context)
    {
        _context = context;
    }

    // Search passenger by passport number
    [HttpGet("search")]
    public async Task<IActionResult> SearchByPassport([FromQuery] string passportNumber)
    {
        var passenger = await _context.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.PassportNumber == passportNumber);

        if (passenger == null) return NotFound();

        return Ok(passenger);
    }

    // Assign seat to passenger
    [HttpPost("{passengerId}/assign-seat")]
    public async Task<IActionResult> AssignSeat(int passengerId, [FromBody] string seatNumber)
    {
        var passenger = await _context.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.Id == passengerId);

        if (passenger == null) return NotFound("Passenger not found");

        var flightId = passenger.FlightId;

        // Check if seat exists on flight
        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);

        if (seat == null) return BadRequest("Seat does not exist");

        if (seat.IsAssigned) return BadRequest("Seat is already taken");

        if (passenger.Seat != null)
        {
            return BadRequest("Passenger already has a seat assigned");
        }

        // Assign seat
        seat.IsAssigned = true;
        seat.PassengerId = passengerId;
        passenger.Seat = seat;

        await _context.SaveChangesAsync();

        // TODO: Notify clients via SignalR (later)

        return Ok(seat);
    }
}
