using IETT.Entity.Entities;
using IETT.Entity.Entitities;
using IETT.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace IETT.DataAccess.Context
{
    public class IETTDbContext : DbContext
    {
        public IETTDbContext(DbContextOptions<IETTDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Garage> Garages { get; set; } = null!;
        public DbSet<Operator> Operators { get; set; } = null!;

        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<DriverStatus> DriverStatuses { get; set; } = null!;
        public DbSet<DriverCertificate> DriverCertificates { get; set; } = null!;
        public DbSet<DriverPerformance> DriverPerformances { get; set; } = null!;

        public DbSet<Inspector> Inspectors { get; set; } = null!;

        public DbSet<Vehicle> Vehicles { get; set; } = null!;

        public DbSet<BusRoute> BusRoutes { get; set; } = null!;
        public DbSet<BusStop> BusStops { get; set; } = null!;
        public DbSet<BusRouteStop> BusRouteStops { get; set; } = null!;

        public DbSet<Trip> Trips { get; set; } = null!;

        public DbSet<ComplaintType> ComplaintTypes { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;

        public DbSet<Investigation> Investigations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DriverStatuses tablosunun başlangıç kayıtları
            modelBuilder.Entity<DriverStatus>().HasData(
                new DriverStatus
                {
                    Id = (int)DriverStatusEnum.Working,
                    StatusName = "Working"
                },
                new DriverStatus
                {
                    Id = (int)DriverStatusEnum.OnLeave,
                    StatusName = "On Leave"
                },
                new DriverStatus
                {
                    Id = (int)DriverStatusEnum.OffDuty,
                    StatusName = "Off Duty"
                },
                new DriverStatus
                {
                    Id = (int)DriverStatusEnum.OnTrip,
                    StatusName = "On Trip"
                }
            );

            // Enum değerleri SQL tarafında int olarak saklanacak
            modelBuilder.Entity<Complaint>()
                .Property(complaint => complaint.ComplaintStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Trip>()
                .Property(trip => trip.TripStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Vehicle>()
                .Property(vehicle => vehicle.VehicleStatus)
                .HasConversion<int>();

            // Durak koordinatlarının hassasiyeti
            modelBuilder.Entity<BusStop>()
                .Property(stop => stop.Latitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<BusStop>()
                .Property(stop => stop.Longitude)
                .HasPrecision(9, 6);

            // Bağlı kayıtların otomatik silinmesini engeller
            foreach (var foreignKey in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(entity => entity.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}