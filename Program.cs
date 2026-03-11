using CecGradingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddSingleton<DatabaseService>();

// CORS - allow everything (grade.html / admin.html from any origin)
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Railway injects PORT automatically; local default is 5050
var port = Environment.GetEnvironmentVariable("PORT") ?? "5050";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("+=======================================================+");
Console.WriteLine("|      CEC Grading System - ASP.NET Core                |");
Console.WriteLine("+=======================================================+");
Console.WriteLine($"|  Admin  -> http://localhost:{port}/admin.html");
Console.WriteLine($"|  Grade  -> http://localhost:{port}/grade.html");
Console.WriteLine("|  Login  -> admin / cec2024");
Console.WriteLine("|  Press Ctrl+C to stop");
Console.WriteLine("+=======================================================+");
Console.ResetColor();

app.Run();
