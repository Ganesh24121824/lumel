using Microsoft.EntityFrameworkCore;
using SalesAnalyticsData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with PostgreSQL
var connectionString = "Host=::1;Port=5432;Database=postgres;Username=postgres;Password=ganesh24181224";
builder.Services.AddDbContext<SalesAnalyticsContext>(options =>
    options.UseNpgsql(connectionString));

var corsPolicy = "allowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
    policy =>
    {
        policy.WithOrigins("http://localhost:3001").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Ensure database created and run migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalesAnalyticsContext>();
    db.Database.Migrate();
}

// Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales Analytics API v1");
    c.RoutePrefix = string.Empty; // serve at application root
});

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
