using InstagramApiSharp.API;
using unfollowers.Models;
using unfollowers.Services.Dtos;

namespace unfollowers.Services.InstagramService
{
    public interface IInstaService
    {
        Task<Status> Login(string username, string password);
        Task<(Status, List<UnfollowersDto>)> GetNonFollowersDtos();
        Task Logout();
        Task<(Status, List<UserInfo>)> GetFollowersAsync();
        Task<(Status, List<UnfollowerInfo>)> GetUnfollowersAsync();
        Task<(Status, LoggedUserInfoDto)> GetUserInfoAsync();
        Task<(Status, List<UnfollowersDto>)> GetPostsStatistic();
    }
}
