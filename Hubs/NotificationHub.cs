using Microsoft.AspNetCore.SignalR;

namespace Resturant_Menu.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message, int bookingId)
        {
            // Send notification to all connected admin clients
            await Clients.All.SendAsync("ReceiveNotification", message, bookingId);
        }

        public async Task SendBookingNotification(int bookingId, string message, string customerName, List<string> items)
        {
            // Send detailed booking notification to all connected clients
            await Clients.All.SendAsync("ReceiveBookingNotification", new
            {
                BookingId = bookingId,
                Message = message,
                CustomerName = customerName,
                Items = items,
                CreatedAt = DateTime.Now
            });
        }
    }
}
