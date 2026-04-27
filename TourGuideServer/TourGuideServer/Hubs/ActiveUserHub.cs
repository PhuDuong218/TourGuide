using Microsoft.AspNetCore.SignalR;

namespace TourGuideServer.Hubs
{
    public class ActiveUserHub : Hub
    {
        // Sử dụng một biến Static để giữ số lượng người đang online
        public static int CurrentOnlineCount = 0;

        // Khi có người kết nối (Mở app)
        public override async Task OnConnectedAsync()
        {
            CurrentOnlineCount++;


            await Clients.All.SendAsync("UpdateOnlineCount", CurrentOnlineCount);
            await base.OnConnectedAsync();
        }

        // Khi mất kết nối (Tắt app, mất mạng, thoát đa nhiệm)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            CurrentOnlineCount = Math.Max(0, CurrentOnlineCount - 1);
            await Clients.All.SendAsync("UpdateOnlineCount", CurrentOnlineCount);
            await base.OnDisconnectedAsync(exception);
        }
    }
}