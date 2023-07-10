using System.ComponentModel.DataAnnotations;

namespace unfollowers.Models
{
    public class FollowersStatistic
    {
        [Key]
        public string Username { get; set; }
        public List<UserInfo> InitialState { get; set; } //fill this list when first time run application
        public List<UnfollowerInfo> UnfollowMe { get; set; } //list to show
        public string Password { get; set; }
        public string Image { get; set; }
    }

    public class UserInfo
    {
        public long? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] Image { get; set; }
    }

    public class UnfollowerInfo : UserInfo
    {
        public DateTime DateOfUnfollowing { get; set; }
    }
}
