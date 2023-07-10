namespace unfollowers.Services.Dtos
{
    public class LoggedUserInfoDto
    {
        public string Username { get; set; }    
        public byte[] Image { get; set; }
        public long? Followers { get; set; }
        public long? Following { get; set; }
    }
}
