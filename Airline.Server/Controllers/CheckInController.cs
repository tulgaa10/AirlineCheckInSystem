using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// CheckInController angilal ni zorchigchdiin check-in uildeltei holbogdson API endpoint-uudiig zohitsuuldag.
/// Zorchigch haih bolon suudal onooh uildliig hiij guitsedgene.
/// Transaction ashiglan olon hereglegchees negin zoroo suudal songoh uyd gardag asuudliig shiidverlene.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly AirlineDbContext _db;

    /// <summary>
    /// CheckInController-iin constructor.
    /// Ogogdliin sangiin context-iig gadnaas injection hiij avna.
    /// </summary>
    /// <param name="db">Ogogdliin sangiin context</param>
    public CheckInController(AirlineDbContext db) => _db = db;

    /// <summary>
    /// Passport dugaaraar zorchigch haih API endpoint.
    /// Ene ni check-in-ii ajillagaanii ehleliin uildel bolno.
    /// GET /api/checkin/find-passenger/{passport}
    /// </summary>
    /// <param name="passport">Haih zorchigchiin passport dugaar</param>
    /// <returns>Zorchigchiin medeelel esvel NotFound hariu</returns>
    [HttpGet("find-passenger/{passport}")]
    public async Task<IActionResult> FindPassenger(string passport)
    {
        // Passport dugaaraar zorchigchiin medeelliig oloh
        // Mun tuunii suudal bolon nislegiin holbootoi medeelliig tataj avna
        var passenger = await _db.Passengers
            .Include(p => p.Seat)
            .Include(p => p.Flight)
            .FirstOrDefaultAsync(p => p.PassportNumber == passport);

        // Herev zorchigch oldoogui bol 404 NotFound hariu butsaana
        if (passenger == null)
            return NotFound("Passenger not found");

        // Oldson bol zorchigchiin medeelliig butsaana
        return Ok(passenger);
    }

    /// <summary>
    /// Zorchigchid suudal onooh API endpoint.
    /// Transaction ashiglan olon hereglegch negin zoroo suudal songohoos sergeilen concurrency asuudliig shiidverlene.
    /// POST /api/checkin/{passengerId}/assign-seat
    /// </summary>
    /// <param name="passengerId">Zorchigchiin ID dugaar</param>
    /// <param name="seatNumber">Onooh suudliin dugaar</param>
    /// <returns>Onooson tuhain medeelel esvel aldaanii shinj</returns>
    [HttpPost("{passengerId}/assign-seat")]
    public async Task<IActionResult> AssignSeat(int passengerId, [FromBody] string seatNumber)
    {
        // InMemory database ashiglahgui uyd transaction ashiglah
        // [Use transaction except when using InMemory database]
        bool useTransaction = !(_db.Database.ProviderName?.Contains("InMemory") ?? false);
        IDbContextTransaction? transaction = null;

        try
        {
            // Transaction ehluuleh (herev heregtei bol)
            if (useTransaction)
                transaction = await _db.Database.BeginTransactionAsync();

            // Zorchigchiin medeelliig ID-gaar haij oloh
            var passenger = await _db.Passengers
                .Include(p => p.Seat)
                .FirstOrDefaultAsync(p => p.Id == passengerId);

            // Herev zorchigch oldoogui bol aldaanii medeelel butsaah
            if (passenger == null)
                return NotFound("Passenger not found");

            // Herev zorchigch urd ni suudaltai bol aldaanii medeelel butsaah
            if (passenger.Seat != null)
                return BadRequest("Passenger already has a seat assigned");

            // Suudliin dugaaraar suudliig haij oloh
            var seat = await _db.Seats
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);

            // Herev suudal oldoogui bol aldaanii medeelel butsaah
            if (seat == null)
                return BadRequest("Seat not found");

            // Herev suudal urd ni avagdsan bol aldaanii medeelel butsaah
            if (seat.IsAssigned)
                return BadRequest("Seat is already assigned");

            // Herev suudal zorchigchiin nisleg deer baihgui bol aldaanii medeelel butsaah
            if (seat.FlightId != passenger.FlightId)
                return BadRequest("Seat is not on the passenger's flight");

            // Suudliig zorchigchid onooj holboogoog uusgeh
            seat.IsAssigned = true;
            seat.PassengerId = passenger.Id;
            passenger.Seat = seat;

            // Ogogdliin sand hadgalah
            await _db.SaveChangesAsync();

            // Transaction-iig batalgaajuulah (herev heregtei bol)
            if (transaction != null)
                await transaction.CommitAsync();

            // Amjilttai bolson medeeg butsaah
            return Ok("Seat assigned successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrency aldaa garsaniig songogdson suudliig uur hereglegch
            // songoj amjisan aldaa gedgiig todorhoilj transaction-iig butsaana
            if (transaction != null)
                await transaction.RollbackAsync();

            return Conflict("Seat was already assigned to someone else.");
        }
        catch (Exception ex)
        {
            // Busad aldaa garsaig ilruuleh ba transaction-iig butsaah
            if (transaction != null)
                await transaction.RollbackAsync();

            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}