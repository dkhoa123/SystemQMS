using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using SystemQMS.Models;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using SystemQMS.Services.Interfaces;

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

builder.Services.AddScoped<INumberFomularSvc, NumberFomularSvc>();
builder.Services.AddScoped<IStickerSvc, StickerSvc>();
builder.Services.AddScoped<ICounterSvc, CounterSvc>();
builder.Services.AddScoped<IDevSvc, DevSvc>();
builder.Services.AddScoped<IKioskSvc, KioskSvc>();

builder.Services.AddScoped<IServiceSvc, ServiceSvc>();

builder.Services.AddScoped<ICounterSessionSvc, CounterSessionSvc>();
builder.Services.AddScoped<IScheduleSvc, ScheduleSvc>();
builder.Services.AddScoped<ICounterTicketSvc, CounterTicketSvc>();

builder.Services.AddScoped<IQueueSvc, QueueSvc>();
builder.Services.AddScoped<ITransferSvc, TransferSvc>();


builder.Services.AddScoped<IAuthSvc, AuthSvc>();

builder.Services.AddScoped<IWebConfigSvc, WebConfigSvc>();
builder.Services.AddScoped<INewsSvc, NewsSvc>();
builder.Services.AddScoped<INotificationSvc, NotificationSvc>();
builder.Services.AddScoped<IIntegrationSvc, IntegrationSvc>();
builder.Services.AddScoped<ISlotSvc, SlotSvc>();

builder.Services.AddScoped<IDspSvc, DspSvc>();
builder.Services.AddScoped<IMqttPublisherSvc, MqttPublisherSvc>();
builder.Services.AddScoped<MqttClientFactory, MqttClientFactory>();

builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
builder.Services.AddScoped<IUserSvc, UserSvc>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailSvc, EmailSvc>();

// Job nền: tự nhận chuyển quầy quá hạn + mở lại quầy qua ngày mới
builder.Services.AddHostedService<OprBackgroundSvc>();
builder.Services.AddScoped<IDashboardSvc, DashboardSvc>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("ReactPolicy", policy =>
    {
        policy
        .WithOrigins("http://localhost:3000", "https://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("ReactPolicy");

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
