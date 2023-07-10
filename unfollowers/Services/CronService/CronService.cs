using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using Microsoft.EntityFrameworkCore;
using unfollowers.Data;
using unfollowers.Models;
using unfollowers.Services.Dtos;

namespace unfollowers.Services.CronService
{
    public class CronService : ICronService
    {
        private IInstaApi _instaApi;
        private readonly ApplicationDbContext _dbContext;

        public CronService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CheckNewUnfollowers()
        {
            //var allUsers = await _dbContext.FollowersStatistics.ToListAsync();

            //foreach (var user in allUsers)
            //{
            //    var userSession = new UserSessionData
            //    {
            //        UserName = user.Username,
            //        Password = user.Password
            //    };

            //    _instaApi = InstaApiBuilder.CreateBuilder()
            //        .SetUser(userSession)
            //        .Build();

            //    if (!_instaApi.IsUserAuthenticated)
            //    {
            //        var loginResult = await _instaApi.LoginAsync();
            //        if (loginResult.Succeeded)
            //        {
            //            var (status, initialState) = await GetFollowersAsync();


            //            List<UserInfo> newUnfollowers = new List<UserInfo>();

            //            foreach (var oldFollower in user.InitialState)
            //            {
            //                if (initialState.Any(x => x.Username == oldFollower.Username) == false &&
            //                    user.UnfollowMe.Any(x => x.Username == oldFollower.Username) == false)
            //                    user.UnfollowMe.Add(new UnfollowerInfo()
            //                    {
            //                        Image = oldFollower.Image,
            //                        Username = oldFollower.Username,
            //                        UserId = oldFollower.UserId,
            //                        DateOfUnfollowing = DateTime.UtcNow
            //                    });
            //            }

            //            _dbContext.FollowersStatistics.Update(user);

            //            try
            //            {
            //                await _dbContext.SaveChangesAsync();
            //            }
            //            catch (Exception e)
            //            {
            //                continue;
            //            }

            //            await _instaApi.LogoutAsync();
            //            _instaApi = null;
            //        }
            //    }
            //}
        }

        public async Task<(Status, List<UserInfo>)> GetFollowersAsync()
        {
            if (_instaApi == null)
                return (new Status(StatusCodes.Status400BadRequest, "You are not logged in"), new List<UserInfo>());

            var currentUserName = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;

            var followers = await _instaApi.UserProcessor.GetUserFollowersAsync(currentUserName, PaginationParameters.MaxPagesToLoad(1));
            var followersUserNames = followers?.Value.Select(x => x.UserName).ToList();

            var userInfo = new List<UserInfo>();

            foreach (var userName in followersUserNames)
            {
                var user = await _instaApi.UserProcessor.GetUserAsync(userName);
                if (user != null)
                {
                    //var instaUser = await _instaApi.UserProcessor.GetUserAsync(user.Value.UserName);
                    long userId = user.Value.Pk;

                    userInfo.Add(new UserInfo()
                    {
                        UserId = userId,
                        Image = await DownloadImage(user.Value.ProfilePicture),
                        Username = user.Value.UserName,
                    });
                }
            }

            return (new Status(), userInfo);
        }

        static async Task<byte[]> DownloadImage(string imageUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                using (MemoryStream ms = new MemoryStream())
                {
                    await response.Content.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
