using System.Security.Claims;
using IETT.Business.Abstract;
using IETT.DataAccess.Context;
using IETT.Entity.DTOs.Drivers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IETTDbContext _context;
        private readonly IDriverService _driverService;

        public DriversController(
            IETTDbContext context,
            IDriverService driverService)
        {
            _context = context;
            _driverService = driverService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<ActionResult<List<DriverListDto>>> GetAll()
        {
            var driverData = await _context.Drivers
                .AsNoTracking()
                .Select(driver => new
                {
                    driver.Id,
                    driver.PersonnelNumber,
                    driver.HolidayDay,

                    driver.User.FirstName,
                    driver.User.LastName,
                    driver.User.IdentityNumber,

                    driver.Garage.GarageName,
                    driver.Operator.OperatorName,

                    DriverStatusName =
                        driver.DriverStatus.StatusName
                })
                .ToListAsync();

            var drivers = driverData
                .Select(driver => new DriverListDto
                {
                    Id = driver.Id,

                    FullName =
                        driver.FirstName + " " +
                        driver.LastName,

                    MaskedIdentityNumber =
                        MaskIdentityNumber(driver.IdentityNumber),

                    PersonnelNumber = driver.PersonnelNumber,

                    GarageName = driver.GarageName,

                    OperatorName = driver.OperatorName,

                    DriverStatusName =
                        driver.DriverStatusName,

                    HolidayDay =
                        driver.HolidayDay.ToString()
                })
                .ToList();

            return Ok(drivers);
        }

        [HttpGet("me/trips")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTrips()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var trips =
                await _driverService.GetMyTripsAsync(userId);

            return Ok(trips);
        }

        private static string MaskIdentityNumber(
            string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return "***********";
            }

            if (identityNumber.Length < 4)
            {
                return new string('*', identityNumber.Length);
            }

            return identityNumber[..2]
                + new string('*', identityNumber.Length - 4)
                + identityNumber[^2..];
        }
    }
}
