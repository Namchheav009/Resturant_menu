using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resturant_Menu.Services
{
    public interface ITelegramService
    {
        Task SendBookingNotificationAsync(string message, List<string>? photoPathsOrUrls = null);
    }
}
