using System.ComponentModel.DataAnnotations;

namespace SocialNetworkProject_3_22_05.Models
{
    public class LoginViewModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
