using Microsoft.AspNetCore.Identity;

namespace SocialNetworkProject_3_22_05.Entities
{
    public class CustomIdentityUser:IdentityUser
    {
        public string?ImageUrl { get; set; }
        public bool IsOnline { get; set; }
        public DateTime DisconnectTime { get; set; } = DateTime.Now;
        public string? ConnectTime { get; set; } = "";
    }
}
