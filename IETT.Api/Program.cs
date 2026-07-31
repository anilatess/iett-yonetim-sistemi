using System.Text;
using IETT.Business.Abstract;
using IETT.Business.Concrete;
using IETT.DataAccess.Abstract;
using IETT.DataAccess.Concrete;
using IETT.DataAccess.Concrete.EntityFramework;
using IETT.DataAccess.Context;
using IETT.Entity.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controller servisleri
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IETT Yönetim Sistemi API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token deðerini giriniz."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
});

// Veritabaný baðlantýsý
builder.Services.AddDbContext<IETTDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Kullanýcý ve giriþ servisleri
builder.Services.AddScoped<IUserDal, EfUserDal>();
builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<ITokenService, TokenManager>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Araç servisleri
builder.Services.AddScoped<IVehicleDal, EfVehicleDal>();
builder.Services.AddScoped<IVehicleService, VehicleManager>();

// Hat servisleri
builder.Services.AddScoped<IBusRouteDal, EfBusRouteDal>();
builder.Services.AddScoped<IBusRouteService, BusRouteManager>();

// Driver servisleri
builder.Services.AddScoped<IDriverService, DriverManager>();

// JWT doðrulama ayarlarý
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key appsettings.json içerisinde bulunamadý."
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer appsettings.json içerisinde bulunamadý."
    );

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience appsettings.json içerisinde bulunamadý."
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

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// React uygulamasýnýn API'ye eriþebilmesi için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

// Sýralama önemlidir
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();