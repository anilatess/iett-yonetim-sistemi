namespace IETT.Entity.DTOs.Drivers
{
    public class DriverListDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string MaskedIdentityNumber { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public string GarageName { get; set; } = string.Empty;

        public string OperatorName { get; set; } = string.Empty;

        public string DriverStatusName { get; set; } = string.Empty;

        public string HolidayDay { get; set; } = string.Empty;
    }
}