/// <summary>
/// Nislegiin Burtgeliin Programmiin Ündsen Angilal.
/// Ene program ni nislegiin burtgel hiih systemiin server taliig ehluulj, 
/// uildverlel, uil ajillagaag todorhoildog.
/// </summary>
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// container luu services nemeh
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // JSON neriig camelCase bolgoj haruulah
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// API-d swagger uusgeh
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db iin holboltuudiig tohiruulah
builder.Services.AddDbContext<AirlineDbContext>(options =>
    // Test hiih bolood hyzgaarlalt shalgahin tuld sanah oig ashiglah
    options.UseInMemoryDatabase("AirlineDatabase"));

// Real-time hariltsaanii SignalR nemeh
builder.Services.AddSignalR();

// Hariultin hemjeeg bagsgahin tuld Response compression ashiglah
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

// Repository pattern ashiglan ogogdol ruu handah
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// Blazor-WASM client-tai holbogdohin tuld CORS tohiruulga hiih
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        // Test hiih uyd ali ch erh zovshooroh
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// HTTP hüselt durem tohiruulah
if (app.Environment.IsDevelopment())
{
    // swagger ashiglah
    app.UseSwagger();
    app.UseSwaggerUI();

    // seed data g db d oruulah
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AirlineDbContext>();
        SeedData(dbContext);
    }
}

// Hariu ogogdliig shahhin tuld Response compression ashiglah
app.UseResponseCompression();

// HTTPS ruu chigluuleh
app.UseHttpsRedirection();

// Erhiin shinalganaas omno CORS-iig ashiglah
app.UseCors("BlazorPolicy");

// Erh zovshoorol shalgah
app.UseAuthorization();

// Controller-uudiin zam zaahin tohiruulga
app.MapControllers();

// SignalR Hub-iin zam tohiruulga
app.MapHub<FlightHub>("/flighthub");

app.Run();

/// <summary>
/// Ene funkts ni niisleluud, suudaluud, zorchinii tuhai jishee medeellig
/// ogogdliin sand oruuldag.
/// </summary>
/// <param name="context"></param>
void SeedData(AirlineDbContext context)
{
    // Niisleguud bga esehiig shalgah
    if (!context.Flights.Any())
    {
        // Jishee niisleg uusgeh
        // [Create sample flights]
        var flight1 = new Flight
        {
            FlightNumber = "oo000",
            DepartureTime = DateTime.Now.AddHours(2),
            Status = FlightStatus.Registering
        };

        var flight2 = new Flight
        {
            FlightNumber = "pp111",
            DepartureTime = DateTime.Now.AddHours(3),
            Status = FlightStatus.Boarding
        };

        context.Flights.AddRange(flight1, flight2);
        context.SaveChanges();

        // Ehnii niislegiin suudaluudiig uusgeh
        var seats1 = new List<Seat>
        {
            new Seat { SeatNumber = "pp", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "aa", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "uu", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "qq", IsAssigned = false, FlightId = flight1.Id }
        };

        // Hoyrdahi niislegiin suudaluudiig uusgeh
        var seats2 = new List<Seat>
        {
            new Seat { SeatNumber = "rr", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "tt", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "yy", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "uu", IsAssigned = false, FlightId = flight2.Id }
        };

        context.Seats.AddRange(seats1);
        context.Seats.AddRange(seats2);

        // Jishee zochin uusgeh
        var passenger1 = new Passenger
        {
            FullName = "bataa",
            PassportNumber = "2000",
            FlightId = flight1.Id
        };
        var passenger2 = new Passenger
        {
            FullName = "bold",
            PassportNumber = "1000",
            FlightId = flight1.Id
        };

        context.Passengers.Add(passenger1);
        context.Passengers.Add(passenger2);

        // Ogogdol hadgalah
        context.SaveChanges();
    }
}