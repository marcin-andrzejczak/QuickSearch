using QuickSearch.Setup;
using QuickSearch.Tests.Api.Common.Data;
using Microsoft.EntityFrameworkCore;
using QuickSearch.Extensions.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.DescribeAllParametersInCamelCase();
    options.AddQuickSearch();
});
builder.Services.AddDbContext<AppDbContext>(
    o => o.UseInMemoryDatabase("paged-mvc-db"),
    ServiceLifetime.Transient,
    ServiceLifetime.Transient
);

// QuickSearch setup
builder.Services.AddQuickSearch();

var app = builder.Build();

if (!bool.TryParse(app.Configuration["DISABLE_SEED"], out var disableSeed) || !disableSeed)
    DatabaseSeeder.SeedDatabase<AppDbContext>(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.AddQuickSearch();
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program
{
}