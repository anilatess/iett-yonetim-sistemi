using IETT.Entity.Entities;
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

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<VehicleStatus> VehicleStatuses { get; set; } = null!;

        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<DriverStatus> DriverStatuses { get; set; } = null!;
        public DbSet<DriverCertificate> DriverCertificates { get; set; } = null!;
        public DbSet<DriverPerformance> DriverPerformances { get; set; } = null!;

        public DbSet<Inspector> Inspectors { get; set; } = null!;

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

            // Enum değerlerini SQL tarafında int olarak saklar
            modelBuilder.Entity<Complaint>()
                .Property(complaint => complaint.ComplaintStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Complaint>()
                .Property(complaint => complaint.TrackingCode)
                .HasMaxLength(30)
                .UseCollation("Turkish_100_CI_AS");

            modelBuilder.Entity<Complaint>()
                .HasIndex(complaint => complaint.TrackingCode)
                .IsUnique()
                .HasDatabaseName("UX_Complaints_TrackingCode");

            modelBuilder.Entity<Investigation>()
                .Property(investigation => investigation.ProcessStatus)
                .HasConversion<int>();

            modelBuilder.Entity<Investigation>()
                .Property(investigation => investigation.DriverExplanation)
                .HasMaxLength(1000);

            modelBuilder.Entity<Investigation>()
                .Property(investigation => investigation.InvestigationResult)
                .HasMaxLength(1000);

            modelBuilder.Entity<Trip>()
                .Property(trip => trip.TripStatus)
                .HasConversion<int>();

            modelBuilder.Entity<DriverCertificate>()
                .Property(certificate => certificate.CertificateType)
                .HasMaxLength(100);

            modelBuilder.Entity<DriverCertificate>()
                .Property(certificate => certificate.FilePath)
                .HasMaxLength(500);

            modelBuilder.Entity<DriverCertificate>()
                .Property(certificate => certificate.OriginalFileName)
                .HasMaxLength(255);

            modelBuilder.Entity<DriverCertificate>()
                .Property(certificate => certificate.ApprovalStatus)
                .HasConversion<int>();

            modelBuilder.Entity<DriverCertificate>()
                .Property(certificate => certificate.RejectionReason)
                .HasMaxLength(500);

            modelBuilder.Entity<DriverCertificate>()
                .HasOne(certificate => certificate.ReviewedByInspector)
                .WithMany()
                .HasForeignKey(certificate => certificate.ReviewedByInspectorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Trip ile BusRoute arasındaki ilişki
            modelBuilder.Entity<Trip>()
                .HasOne(trip => trip.BusRoute)
                .WithMany(route => route.Trips)
                .HasForeignKey(trip => trip.RouteId)
                .OnDelete(DeleteBehavior.NoAction);

            // Complaint ile BusRoute arasındaki ilişki
            modelBuilder.Entity<Complaint>()
                .HasOne(complaint => complaint.BusRoute)
                .WithMany(route => route.Complaints)
                .HasForeignKey(complaint => complaint.RouteId)
                .OnDelete(DeleteBehavior.NoAction);

            // Complaint ile BusStop arasındaki ilişki
            modelBuilder.Entity<Complaint>()
                .HasOne(complaint => complaint.BusStop)
                .WithMany(stop => stop.Complaints)
                .HasForeignKey(complaint => complaint.StopId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Vehicle ile VehicleStatus arasındaki ilişki
            modelBuilder.Entity<Vehicle>()
                .HasOne(vehicle => vehicle.VehicleStatus)
                .WithMany(status => status.Vehicles)
                .HasForeignKey(vehicle => vehicle.VehicleStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Vehicle>()
                .Property(vehicle => vehicle.LicensePlate)
                .HasMaxLength(20);

            modelBuilder.Entity<Vehicle>()
                .Property(vehicle => vehicle.Model)
                .HasMaxLength(150);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(vehicle => vehicle.LicensePlate)
                .IsUnique();

            // Durak koordinatlarının hassasiyeti
            modelBuilder.Entity<BusStop>()
                .Property(stop => stop.Latitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<BusStop>()
                .Property(stop => stop.Longitude)
                .HasPrecision(9, 6);

            // Bağlı kayıtların otomatik olarak zincirleme silinmesini engeller
            foreach (var foreignKey in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(entity => entity.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}
