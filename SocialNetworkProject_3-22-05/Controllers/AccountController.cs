using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkProject_3_22_05.Data;
using SocialNetworkProject_3_22_05.Entities;
using SocialNetworkProject_3_22_05.Models;
using SocialNetworkProject_3_22_05.Services;

namespace SocialNetworkProject_3_22_05.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly RoleManager<CustomIdentityRole> _roleManager;
        private readonly SignInManager<CustomIdentityUser> _signInManager;
        private readonly SocialNetworkDbContext _context;
        private readonly IImageService _imageService;

        public AccountController(UserManager<CustomIdentityUser> userManager, RoleManager<CustomIdentityRole> roleManager, SignInManager<CustomIdentityUser> signInManager, SocialNetworkDbContext context, IImageService imageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
            _imageService = imageService;
        }

        // GET: AccountController
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.File != null)
                {
                    model.ImageUrl=await _imageService.SaveFileAsync(model.File);
                }

                CustomIdentityUser user = new CustomIdentityUser
                {
                    UserName=model.Username,
                    Email=model.Email,
                    ImageUrl=model.ImageUrl,
                };

                IdentityResult result=await _userManager.CreateAsync(user,model.Password);
                if(result.Succeeded)
                {
                    if(!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        CustomIdentityRole role = new CustomIdentityRole
                        {
                            Name="Admin"
                        };

                        IdentityResult roleResult = await _roleManager.CreateAsync(role);
                        if(!roleResult.Succeeded)
                        {
                            ModelState.AddModelError("", "We can not add the role!");
                        }
                    }
                }
                await _userManager.AddToRoleAsync(user, "Admin");
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user=_context.Users.SingleOrDefault(u=>u.UserName==model.Username);
                    user.IsOnline = true;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid Login");
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            var user=await _userManager.GetUserAsync(HttpContext.User);
            user.DisconnectTime=DateTime.Now;
            user.IsOnline = false;
            await _context.SaveChangesAsync();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }
}
