using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using SystemQMS.Hubs;
using SystemQMS.Models;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using SystemQMS.Middleware;
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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT của bạn"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddScoped<ICounterSvc, CounterSvc>();
builder.Services.AddScoped<ICounterSessionSvc, CounterSessionSvc>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<KeypadConnectionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenRevocationService,TokenRevocationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Thiếu Jwt:Key");

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
       
        options.UseSecurityTokenValidators = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                System.Diagnostics.Debug.WriteLine($"FAIL: {context.Exception.Message}");
                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                
                string? token = (context.SecurityToken as JwtSecurityToken)?.RawData;

                if (string.IsNullOrEmpty(token))
                {
                    context.Fail("Token không hợp lệ.");
                    return;
                }

                var service = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocationService>();

                bool isRevoked = await service.IsTokenRevokedAsync(token);
                if (isRevoked)
                {
                    context.Fail("Token đã bị thu hồi.");
                }
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    statusCode = 401,
                    message = context.ErrorDescription ?? "Token không hợp lệ hoặc đã hết hạn.",
                    error = context.Error ?? "invalid_token"
                });

                await context.Response.WriteAsync(result);
            }
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();
//app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


    app.UseStaticFiles();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<SessionHub>("/sessionHub");

    app.Run();
