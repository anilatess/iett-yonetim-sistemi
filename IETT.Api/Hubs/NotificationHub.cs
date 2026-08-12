using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IETT.Api.Hubs
{
    [Authorize(Roles = "Driver,Inspector")]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                Context.Abort();
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);
            var groupName = role == "Inspector"
                ? NotificationGroupNames.ForInspectorUser(userId)
                : NotificationGroupNames.ForDriverUser(userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await base.OnConnectedAsync();
        }
    }
}
