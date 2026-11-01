using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;
using SystemQMS.Models;
using SystemQMS.Services;
using SystemQMS.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration configuration = new ConfigurationBuilder().SetBasePath(
    Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("ConnectionDb")));

builder.Services.AddScoped<IDevSvc, DevSvc>();
builder.Services.AddControllers();

builder.Services.AddSignalR();
builder.Services.AddScoped<IConfigSvc, ConfigService>();
builder.Services.AddScoped<IAudioMappingSvc, AudioMappingService>();

builder.Services.AddScoped<INumberFomularSvc, NumberFomularSvc>();
builder.Services.AddScoped<IStickerSvc, StickerSvc>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICounterSvc, CounterSvc>();
builder.Services.AddScoped<ICounterSessionSvc, CounterSessionSvc>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<KeypadConnectionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseRouting();

app.MapControllers();
app.MapHub<SessionHub>("/sessionHub");

app.Run();
