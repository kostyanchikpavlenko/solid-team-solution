using System.Text.Json;
using Application.API.Interfaces;
using Application.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/move", (MoveRequest request, HttpResponse response, IGameService gameService) =>
{
    // Validate the input
    if (request == null || request.Field == null || request.Field.Length == 0)
    {
        return Results.BadRequest(new { Message = "Invalid request data." });
    }
    
    var nextStep = gameService.GetNextMove(request.Field, request.NarrowingIn);
    response.ContentType = "application/json";
    return Results.Ok(new { move = nextStep });
});

///Test endpoint
app.MapGet("/healthz", (HttpResponse response) =>
{
    response.ContentType = "application/json";
    return JsonSerializer.Serialize(new { status = "OK" });
});

app.Run();


public class MoveRequest
{
    public string[][] Field { get; set; } = new string[][]{};
    public int NarrowingIn { get; set; }
    public int GameId { get; set; }
}


