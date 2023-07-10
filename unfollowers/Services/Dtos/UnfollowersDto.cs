namespace unfollowers.Services.Dtos
{
    public class UnfollowersDto
    {
        public long? UserId { get; set; } 
        public string Username { get; set; } = string.Empty;
        public byte[] Image { get; set; }
        public bool IFollow { get; set; }
        public bool FollowMe { get; set; }
        public int? TotalPostsLiked { get; set; }
    }
}
