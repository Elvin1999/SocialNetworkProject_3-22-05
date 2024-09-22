using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkProject_3_22_05.Data;
using SocialNetworkProject_3_22_05.Entities;
using SocialNetworkProject_3_22_05.Models;
using System.Diagnostics;

namespace SocialNetworkProject_3_22_05.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SocialNetworkDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<CustomIdentityUser> userManager, SocialNetworkDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = user;
            return View();
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var myrequests = _context.FriendRequests.Where(r => r.SenderId == user.Id);

            var myfriends = _context.Friends.Where(f => f.OwnId == user.Id || f.YourFriendId == user.Id);

            var users = await _context.Users
                .Where(u => u.Id != user.Id)
                .OrderByDescending(u => u.IsOnline)
                .Select(u => new CustomIdentityUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    IsOnline = u.IsOnline,
                    ImageUrl = u.ImageUrl,
                    Email = u.Email,
                    HasRequestPending = (myrequests.FirstOrDefault(r => r.ReceiverId == u.Id && r.Status == "Request") != null),
                    IsFriend = myfriends.FirstOrDefault(f => f.OwnId == u.Id || f.YourFriendId == u.Id) != null
                })
                .ToListAsync();
            return Ok(users);
        }
        [HttpDelete()]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            try
            {
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == id);
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> DeclineRequest(int id, string senderId)
        {
            try
            {
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == id);
                _context.FriendRequests.Remove(request);

                var current = await _userManager.GetUserAsync(HttpContext.User);
                _context.FriendRequests.Add(new FriendRequest
                {
                    SenderId = current.Id,
                    Sender = current,
                    ReceiverId = senderId,
                    Status = "Notification",
                    Content = $"{current.UserName} declined your friend request at {DateTime.Now.ToLongDateString()} {DateTime.Now.ToShortTimeString()}"
                });

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> SendFollow(string id)
        {
            var sender = await _userManager.GetUserAsync(HttpContext.User);
            var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (receiverUser != null)
            {
                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} sent friend request at {DateTime.Now.ToLongDateString()}",
                    SenderId = sender.Id,
                    Sender = sender,
                    ReceiverId = id,
                    Status = "Request"
                });
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        public async Task<IActionResult> AcceptRequest(string userId, string senderId, int requestId)
        {
            var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var sender = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == senderId);
            if (receiverUser != null)
            {
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId);
                _context.FriendRequests.Remove(request);

                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} accepted friend request at {DateTime.Now.ToLongDateString()} {DateTime.Now.ToShortTimeString()}",
                    SenderId = senderId,
                    ReceiverId = receiverUser.Id,
                    Sender = sender,
                    Status = "Notification"
                });

                _context.Friends.Add(new Friend
                {
                    OwnId = sender.Id,
                    YourFriendId = receiverUser.Id,
                });

                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> GetAllRequests()
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var requests = _context.FriendRequests.Where(r => r.ReceiverId == current.Id);
            return Ok(requests);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
