using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkProject_3_22_05.Data;
using SocialNetworkProject_3_22_05.Entities;
using SocialNetworkProject_3_22_05.Models;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

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

        public async Task<IActionResult> GoChat(string id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var chat = await _context.Chats.Include(nameof(Chat.Messages)).FirstOrDefaultAsync(c => c.SenderId == user.Id && c.ReceiverId == id ||
            c.SenderId == id && c.ReceiverId == user.Id);

            if (chat == null)
            {
                chat = new Chat
                {
                    ReceiverId = id,
                    SenderId = user.Id,
                    Messages = new List<Message>()
                };

                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
            }

            var messages = chat.Messages;
            if(messages.Any())
            {
                foreach (var item in messages)
                {
                    item.HasSeen = true;
                    _context.Messages.Update(item);
                }
            }

            await _context.SaveChangesAsync();

            var chats = _context.Chats.Include(nameof(chat.Messages)).Include(nameof(Chat.Receiver)).Where(c => c.SenderId == user.Id || c.ReceiverId == user.Id);
            var chatBlocks = from c in chats
                             let receiver = (user.Id != c.ReceiverId) ? c.Receiver : _context.Users.FirstOrDefault(u => u.Id == c.SenderId)
                             select new ChatBlockViewModel
                             {
                                 Messages = c.Messages,
                                 Id = c.Id,
                                 SenderId = c.SenderId,
                                 Receiver = receiver,
                                 ReceiverId = receiver.Id,
                                 UnReadMessageCount = c.Messages.Count(m => m.HasSeen == false)
                             };

            var result = chatBlocks.ToList().Where(c => c.ReceiverId != user.Id);

            var currentChatBlock = new ChatBlockViewModel
            {
                Id = chat.Id,
                Messages = messages,
                Receiver = chat.Receiver,
                ReceiverId = chat.ReceiverId,
                SenderId = chat.SenderId,
                UnReadMessageCount = chat.Messages.Count(m => m.HasSeen == false)
            };

            var model = new ChatViewModel
            {
                CurrentUserId = user.Id,
                CurrentReceiver = id,
                CurrentChat = currentChatBlock,
                Chats = result.Count() == 0 ? chatBlocks : result,
            };

            return View(model);
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

        [HttpDelete]
        public async Task<IActionResult> TakeRequest(string id)
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.SenderId == current.Id && r.ReceiverId == id);
            if (request == null) return NotFound();
            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Unfollow(string id)
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var friend = await _context.Friends.FirstOrDefaultAsync(f => f.OwnId == current.Id && f.YourFriendId == id ||
            f.OwnId == id && f.YourFriendId == current.Id
            );

            if (friend == null) return NotFound();
            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();
            return Ok();
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

        [HttpPost]
        public async Task<IActionResult> AddMessage(MessageModel model)
        {
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.SenderId == model.SenderId && c.ReceiverId == model.ReceiverId
            || c.SenderId == model.ReceiverId && c.ReceiverId == model.SenderId);
            if (chat != null)
            {
                var message = new Message
                {
                    ChatId = chat.Id,
                    Content = model.Content,
                    DateTime = DateTime.Now,
                    HasSeen = false,
                    IsImage = false,
                    SenderId = current.Id
                };
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        public async Task<IActionResult> GetAllMessages(string receiverId, string senderId)
        {
            var chat = await _context.Chats.Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.SenderId == senderId && c.ReceiverId == receiverId
            || c.SenderId == receiverId && c.ReceiverId == senderId);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return Ok(new { Messages = chat.Messages != null ? chat.Messages : new List<Message>(), CurrentUserId = user.Id });
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
