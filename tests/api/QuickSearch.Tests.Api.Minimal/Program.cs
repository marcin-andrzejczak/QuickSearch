using QuickSearch.Tests.Api.Common.Data;
using Microsoft.EntityFrameworkCore;
using QuickSearch.Pagination;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("paged-minimal-db"));

var app = builder.Build();

DatabaseSeeder.SeedDatabase<AppDbContext>(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/users", (AppDbContext context) => 
    context.Users.ToListAsync()
).WithName("GetUsers");

app.MapGet("/users/paged", (AppDbContext context, int page, int pageSize) => 
    context.Users.ToPageAsync(new PageOptions {
        Number = page,
        Size = pageSize
    })
).WithName("GetUsersPaged");

app.Run();
