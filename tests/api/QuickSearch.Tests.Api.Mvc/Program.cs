using QuickSearch.Setup;
using QuickSearch.Tests.Api.Common.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.DescribeAllParametersInCamelCase());
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program
{
}