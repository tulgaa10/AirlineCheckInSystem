using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<AirlineDbContext>(options =>
    options.UseInMemoryDatabase("AirlineDatabase"));

// Add SignalR with response compression
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

// Register repositories
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// Configure CORS for Blazor WASM client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed the database with some sample data
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AirlineDbContext>();
        SeedData(dbContext);
    }
}

app.UseResponseCompression();
app.UseHttpsRedirection();

// Apply CORS before authorization
app.UseCors("BlazorPolicy");

app.UseAuthorization();

app.MapControllers();
app.MapHub<FlightHub>("/flighthub");

app.Run();

// Method to seed initial data
void SeedData(AirlineDbContext context)
{
    // Only seed if no flights exist
    if (!context.Flights.Any())
    {
        // Create a sample flight
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

        // Create seats for flight 1
        var seats1 = new List<Seat>
        {
            new Seat { SeatNumber = "pp", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "aa", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "uu", IsAssigned = false, FlightId = flight1.Id },
            new Seat { SeatNumber = "qq", IsAssigned = false, FlightId = flight1.Id }
        };
        
        var seats2 = new List<Seat>
        {
            new Seat { SeatNumber = "rr", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "tt", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "yy", IsAssigned = false, FlightId = flight2.Id },
            new Seat { SeatNumber = "uu", IsAssigned = false, FlightId = flight2.Id }
        };

        context.Seats.AddRange(seats1);
        context.Seats.AddRange(seats2);

        // Create a sample passenger
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

        context.SaveChanges();
    }
}