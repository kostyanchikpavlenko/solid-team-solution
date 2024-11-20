using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/move", (MoveRequest request) =>
{
    // Validate the input
    if (request == null || request.Field == null || request.Field.Count == 0)
    {
        return Results.BadRequest(new { Message = "Invalid request data." });
    }

    // Process the request (example logic)
    Console.WriteLine($"Game ID: {request.GameId}");
    Console.WriteLine($"Narrowing In: {request.NarrowingIn}");
    Console.WriteLine("Field:");
    foreach (var row in request.Field)
    {
        Console.WriteLine(string.Join(", ", row));
    }

    // Return success response
    return Results.Ok(new { Message = "Move processed successfully." });
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
    public List<List<string>> Field { get; set; } = new();
    public int NarrowingIn { get; set; }
    public int GameId { get; set; }
}


