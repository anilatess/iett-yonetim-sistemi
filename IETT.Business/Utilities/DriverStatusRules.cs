using IETT.Entity.Enums;

namespace IETT.Business.Utilities
{
    internal static class DriverStatusRules
    {
        public static bool CanReceiveTrip(int driverStatusId) =>
            driverStatusId is (int)DriverStatusEnum.Working
                or (int)DriverStatusEnum.OnTrip;

        public static string GetEffectiveStatusName(
            int driverStatusId,
            bool hasActiveTrip)
        {
            if (hasActiveTrip)
            {
                return "Seferde";
            }

            return driverStatusId switch
            {
                (int)DriverStatusEnum.Working => "Çalışıyor",
                (int)DriverStatusEnum.OnLeave => "İzinli",
                (int)DriverStatusEnum.OffDuty => "Görev Dışı",
                (int)DriverStatusEnum.OnTrip => "Çalışıyor",
                _ => "Bilinmiyor"
            };
        }
    }
}
