using Airline.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly AirlineDbContext _db;

    public CheckInController(AirlineDbContext db) => _db = db;

    [HttpGet("find-passenger/{passport}")]
    public async Task<IActionResult> FindPassenger(string passport)
    {
        var passenger = await _db.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.PassportNumber == passport);

        if (passenger == null)
            return NotFound("Passenger not found");

        return Ok(passenger);
    }

    [HttpPost("{passengerId}/assign-seat")]
    public async Task<IActionResult> AssignSeat(int passengerId, [FromBody] string seatNumber)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);
        if (seat == null || seat.IsAssigned)
            return BadRequest("Seat is not available");
        var passenger = await _db.Passengers.Include(p => p.Seat).FirstOrDefaultAsync(p => p.Id == passengerId);
        if (passenger == null)
            return NotFound("Passenger not found");
        try
        {
            seat.IsAssigned = true;
            seat.PassengerId = passenger.Id;
            await _db.SaveChangesAsync();
            return Ok("Seat assigned");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Seat was already assigned to someone else.");
        }
    }

}
