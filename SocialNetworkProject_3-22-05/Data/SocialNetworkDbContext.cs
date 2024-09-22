using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetworkProject_3_22_05.Entities;

namespace SocialNetworkProject_3_22_05.Data
{
    public class SocialNetworkDbContext:IdentityDbContext<CustomIdentityUser,CustomIdentityRole,string>
    {
        public SocialNetworkDbContext(DbContextOptions<SocialNetworkDbContext> options)
            :base(options) { }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
    }
}
