using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkProject_3_22_05.Data;
using SocialNetworkProject_3_22_05.Entities;

namespace SocialNetworkProject_3_22_05.Hubs
{
    public class ChatHub:Hub
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SocialNetworkDbContext _context;

        public ChatHub(UserManager<CustomIdentityUser> userManager, IHttpContextAccessor contextAccessor, SocialNetworkDbContext context)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var userItem = await _context.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            userItem.IsOnline = true;
            userItem.ConnectTime=DateTime.Now.ToLongDateString()+" "+DateTime.Now.ToLongTimeString();
            await _context.SaveChangesAsync();

            string info = user.UserName + " connected successfully";
            await Clients.Others.SendAsync("Connect", info);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var userItem = await _context.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            userItem.IsOnline = false;
            userItem.DisconnectTime = DateTime.Now;
            await _context.SaveChangesAsync();
            string info = user.UserName + " disconnected successfully";
            await Clients.Others.SendAsync("Disconnect", info);
        }

    }
}
