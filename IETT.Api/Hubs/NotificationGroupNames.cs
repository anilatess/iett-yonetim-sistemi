namespace IETT.Api.Hubs
{
    public static class NotificationGroupNames
    {
        public static string ForDriverUser(int userId) => $"driver-user:{userId}";
        public static string ForInspectorUser(int userId) => $"inspector-user:{userId}";
    }
}
