using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        bool useTransaction = !(_db.Database.ProviderName?.Contains("InMemory") ?? false);
        IDbContextTransaction? transaction = null;

        try
        {
            if (useTransaction)
                transaction = await _db.Database.BeginTransactionAsync();

            var passenger = await _db.Passengers
                .Include(p => p.Seat)
                .FirstOrDefaultAsync(p => p.Id == passengerId);

            if (passenger == null)
                return NotFound("Passenger not found");

            if (passenger.Seat != null)
                return BadRequest("Passenger already has a seat assigned");

            var seat = await _db.Seats
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);

            if (seat == null)
                return BadRequest("Seat not found");

            if (seat.IsAssigned)
                return BadRequest("Seat is already assigned");

            if (seat.FlightId != passenger.FlightId)
                return BadRequest("Seat is not on the passenger's flight");

            seat.IsAssigned = true;
            seat.PassengerId = passenger.Id;
            passenger.Seat = seat;

            await _db.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();

            return Ok("Seat assigned successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            return Conflict("Seat was already assigned to someone else.");
        }
        catch (Exception ex)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
