﻿using Microsoft.AspNetCore.Identity;

namespace SocialNetworkProject_3_22_05.Entities
{
    public class CustomIdentityUser:IdentityUser
    {
        public string?ImageUrl { get; set; }
        public bool IsFriend { get; set; }
        public bool HasRequestPending { get; set; }
        public bool IsOnline { get; set; }
        public DateTime DisconnectTime { get; set; } = DateTime.Now;
        public string? ConnectTime { get; set; } = "";
        public virtual ICollection<Friend>? Friends { get; set; }
        public virtual ICollection<FriendRequest>? FriendRequests { get; set; }
        public virtual ICollection<Chat>? Chats { get; set; }
        public CustomIdentityUser()
        {
            Friends=new List<Friend>(); 
            FriendRequests=new List<FriendRequest>();
            Chats=new List<Chat>();
        }
    }
}
