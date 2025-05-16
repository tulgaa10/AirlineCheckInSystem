using Airline.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext using SQLite ¡ª must NOT be commented out!
builder.Services.AddDbContext<AirlineDbContext>(options =>
    options.UseSqlite("Data Source=airline.db"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Airline API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapHub<FlightHub>("/flighthub");

app.Run();
