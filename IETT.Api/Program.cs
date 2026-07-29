using IETT.Business.Abstract;
using IETT.Business.Concrete;
using IETT.DataAccess.Abstract;
using IETT.DataAccess.Concrete;
using IETT.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controller servisleri
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabaný baðlantýsý
builder.Services.AddDbContext<IETTDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Araç servisleri
builder.Services.AddScoped<IVehicleDal, EfVehicleDal>();
builder.Services.AddScoped<IVehicleService, VehicleManager>();

// Hat servisleri
builder.Services.AddScoped<IBusRouteDal, EfBusRouteDal>();
builder.Services.AddScoped<IBusRouteService, BusRouteManager>();

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

// CORS, Authorization'dan önce çalýþmalý
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();