using Microsoft.EntityFrameworkCore;
using PackageTracking.Api.Application;
using PackageTracking.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("packages"));

builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddSingleton<ITrackingNumberGenerator, TrackingNumberGenerator>();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:3000", "https://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

SeedData.Initialize(app.Services, count: 100);

app.Run();
