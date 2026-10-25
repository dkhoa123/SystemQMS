using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration configuration = new ConfigurationBuilder().SetBasePath(
    Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("ConnectionDb")));

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<IConfigSvc, ConfigService>();
builder.Services.AddScoped<IAudioMappingSvc, AudioMappingService>();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
