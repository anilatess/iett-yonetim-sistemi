using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IETT.Api.Hubs
{
    [Authorize(Roles = "Driver")]
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

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                NotificationGroupNames.ForDriverUser(userId));

            await base.OnConnectedAsync();
        }
    }
}
