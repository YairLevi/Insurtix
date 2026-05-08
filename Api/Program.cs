using Api.Books;
using Api.Reports;
using Api.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BookstoreSettings>(builder.Configuration.GetSection("Bookstore"));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
Console.WriteLine($"[CORS] env={builder.Environment.EnvironmentName} origins=[{string.Join(", ", allowedOrigins)}]");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IBooksRepository, FileBooksRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddSingleton<ReportGeneratorFactory>();

var app = builder.Build();

app.UseCors("AllowedOrigins");
app.UseAuthorization();
app.MapControllers();

app.Run();
