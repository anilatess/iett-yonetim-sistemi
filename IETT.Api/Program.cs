using System.Text;
using IETT.Business.Abstract;
using IETT.Business.Concrete;
using IETT.DataAccess.Abstract;
using IETT.DataAccess.Concrete;
using IETT.DataAccess.Concrete.EntityFramework;
using IETT.DataAccess.Context;
using IETT.Entity.Entities;
using IETT.Api.Hubs;
using IETT.Api.Jobs;
using IETT.Business.DeadlineReminders;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error))
                ?? "Gönderilen bilgiler geçersizdir.";

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new { message }
            );
        };
    });

// SignalR
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "IETT Yönetim Sistemi API",
            Version = "v1"
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT token değerini giriniz."
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        }
    );
});

// Database connection
var defaultConnection =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required."
    );

builder.Services.AddDbContext<IETTDbContext>(options =>
    options.UseSqlServer(defaultConnection)
);

// Deadline reminder options
builder.Services
    .AddOptions<InvestigationDeadlineOptions>()
    .Bind(
        builder.Configuration.GetSection(
            InvestigationDeadlineOptions.SectionName
        )
    )
    .ValidateOnStart();

builder.Services.AddSingleton<
    Microsoft.Extensions.Options.IValidateOptions<InvestigationDeadlineOptions>,
    InvestigationDeadlineOptionsValidator
>();

builder.Services.AddSingleton<
    IBusinessDayCalculator,
    BusinessDayCalculator
>();

builder.Services.AddSingleton<
    IInvestigationReminderPolicy,
    InvestigationReminderPolicy
>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<InvestigationDeadlineReminderJob>();

// Hangfire
builder.Services.AddHangfire(configuration =>
    configuration
        .SetDataCompatibilityLevel(
            CompatibilityLevel.Version_180
        )
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            defaultConnection,
            new SqlServerStorageOptions
            {
                SchemaName = "HangFire",
                PrepareSchemaIfNecessary = true
            }
        )
);

builder.Services.AddHangfireServer();

// User / Auth services
builder.Services.AddScoped<IUserDal, EfUserDal>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<ITokenService, TokenManager>();
builder.Services.AddScoped<IUserService, UserManager>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

// Vehicle services
builder.Services.AddScoped<IVehicleDal, EfVehicleDal>();
builder.Services.AddScoped<IVehicleService, VehicleManager>();

// Bus route services
builder.Services.AddScoped<IBusRouteDal, EfBusRouteDal>();
builder.Services.AddScoped<IBusRouteService, BusRouteManager>();

// Driver services
builder.Services.AddScoped<IDriverService, DriverManager>();

// Inspector services
builder.Services.AddScoped<IInspectorService, InspectorManager>();
builder.Services.AddScoped<IInvestigationService, InvestigationManager>();

// Trip services
builder.Services.AddScoped<ITripService, TripManager>();

// Public complaint services
builder.Services.AddScoped<
    IPublicComplaintService,
    PublicComplaintManager
>();

// JWT
var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key appsettings.json içerisinde bulunamadı."
    );

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer appsettings.json içerisinde bulunamadı."
    );

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience appsettings.json içerisinde bulunamadı."
    );

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var requestPath =
                    context.HttpContext.Request.Path;

                if (
                    requestPath.StartsWithSegments(
                        "/hubs/notifications"
                    )
                )
                {
                    var accessToken =
                        context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://iett-yonetim-sistemi-dun.vercel.app"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

// Hangfire recurring job
var deadlineOptions =
    app.Services
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<
                InvestigationDeadlineOptions
            >
        >()
        .Value;

var deadlineTimeZone =
    TimeZoneResolver.Resolve(
        deadlineOptions.TimeZoneId,
        deadlineOptions.WindowsTimeZoneId
    );

app.Services
    .GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<InvestigationDeadlineReminderJob>(
        "investigation-deadline-reminders",
        job => job.ExecuteAsync(),
        deadlineOptions.ScanCron,
        new RecurringJobOptions
        {
            TimeZone = deadlineTimeZone
        }
    );

// Swagger hem local hem production ortamında açık
app.UseSwagger();
app.UseSwaggerUI();

// Hangfire Dashboard sadece local Development ortamında açık
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Render HTTPS'i proxy tarafında yönettiği için
// burada UseHttpsRedirection kullanmıyoruz.

app.UseStaticFiles();

app.UseRouting();

// CORS, authentication'dan önce çalışmalı
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>(
    "/hubs/notifications"
);

app.Run();