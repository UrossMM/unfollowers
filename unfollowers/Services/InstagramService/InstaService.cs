using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Microsoft.EntityFrameworkCore;
using unfollowers.Data;
using unfollowers.Models;
using unfollowers.Services.Dtos;

namespace unfollowers.Services.InstagramService
{
    public class InstaService : IInstaService
    {
        private IInstaApi _instaApi;
        private readonly IServiceScopeFactory _scopeFactory;

        private string CurrentUsername = null;
        private IResult<InstaUserShortList> Followers = null;
        private IResult<InstaUserShortList> Following = null;
        private long FollowersCount = 0;
        private long FollowingCount = 0;

        public InstaService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Status> Login(string username, string password)
        {
            var userSession = new UserSessionData
            {
                UserName = username,
                Password = password
            };

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .Build();

            if (!_instaApi.IsUserAuthenticated)
            {
                var loginResult = await _instaApi.LoginAsync();
                if (loginResult.Succeeded)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var user = await _dbContext.FollowersStatistics.FirstOrDefaultAsync(x => x.Username == username);
                        if (user == null)
                        {
                            var (status, initialState) = await GetFollowersAsync();
                            if (status.Code != StatusCodes.Status200OK)
                                return new Status(StatusCodes.Status400BadRequest, status.ErrorMessage);

                            var instaUser = await _instaApi.UserProcessor.GetUserAsync(username);

                            FollowersStatistic statistic = new FollowersStatistic()
                            {
                                Username = username,
                                InitialState = initialState,
                                UnfollowMe = new List<UnfollowerInfo>(),
                                Password = password,
                                Image = instaUser.Value.ProfilePicture
                            };

                            await _dbContext.FollowersStatistics.AddAsync(statistic);
                        }
                        else
                        {
                            //da li me je neko otpratio od trenutka kada sam se poslednji put logovao?
                            var (status, initialState) = await GetFollowersAsync();
                            if (status.Code != StatusCodes.Status200OK)
                                return new Status(StatusCodes.Status400BadRequest, status.ErrorMessage);

                            List<UserInfo> newUnfollowers = new List<UserInfo>();

                            foreach (var oldFollower in user.InitialState)
                            {
                                if (initialState.Any(x => x.Username == oldFollower.Username) == false &&
                                    user.UnfollowMe.Any(x => x.Username == oldFollower.Username) == false)
                                    user.UnfollowMe.Add(new UnfollowerInfo()
                                    {
                                        Image = oldFollower.Image,
                                        Username = oldFollower.Username,
                                        UserId = oldFollower.UserId,
                                        DateOfUnfollowing = DateTime.UtcNow
                                    });
                            }

                            foreach (var currentFollower in initialState)
                            {
                                if (user.InitialState.Any(x => x.Username == currentFollower.Username) == false)
                                {
                                    user.InitialState.Add(new UserInfo()
                                    {
                                        Username = currentFollower.Username,
                                        Image = currentFollower.Image,
                                        UserId = currentFollower.UserId
                                    });
                                }
                            }

                            _dbContext.FollowersStatistics.Update(user);
                        }

                        try
                        {
                            await _dbContext.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            return new Status(StatusCodes.Status500InternalServerError, "Server error while login user");
                        }

                        return new Status();
                    }
                }
            }

            return new Status(StatusCodes.Status400BadRequest, "Login failed");
        }

        public async Task<(Status, List<UnfollowersDto>)> GetNonFollowersDtos()
        {
            if (_instaApi == null)
                return (new Status(StatusCodes.Status400BadRequest, "You are not logged in"), new List<UnfollowersDto>());

            if (string.IsNullOrWhiteSpace(CurrentUsername))
                CurrentUsername = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;

            if (Followers == null)
                Followers = await _instaApi.UserProcessor.GetUserFollowersAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad((int)(FollowersCount / 50) + 1));

            if (Following == null)
                Following = await _instaApi.UserProcessor.GetUserFollowingAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad((int)(FollowingCount / 50) + 1));

            var followerUserNames = Followers?.Value?.Select(x => x.UserName).ToList();
            var followingUserNames = Following?.Value?.Select(x => x.UserName).ToList();

            var nonFollowers = followingUserNames.Except(followerUserNames).ToList();

            var unfollowersDtos = new List<UnfollowersDto>();
            foreach (var userName in nonFollowers)
            {
                var user = await _instaApi.UserProcessor.GetUserAsync(userName);
                if (user != null)
                {
                    var instaUser = await _instaApi.UserProcessor.GetUserAsync(user.Value?.UserName);
                    long? userId = instaUser?.Value?.Pk;

                    unfollowersDtos.Add(new UnfollowersDto()
                    {
                        UserId = userId,
                        Image = await DownloadImage(user.Value.ProfilePicture),
                        Username = user.Value.UserName,
                        IFollow = followingUserNames.Contains(user.Value.UserName)
                    });
                }
            }

            return (new Status(), unfollowersDtos);
        }

        public Task Logout()
        {
            _instaApi = null;

            return Task.CompletedTask;
        }

        public async Task<(Status, List<UserInfo>)> GetFollowersAsync()
        {
            if (_instaApi == null)
                return (new Status(StatusCodes.Status400BadRequest, "You are not logged in"), new List<UserInfo>());

            if (string.IsNullOrWhiteSpace(CurrentUsername))
                CurrentUsername = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;

            if (Followers == null)
                Followers = await _instaApi.UserProcessor.GetUserFollowersAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad((int)(FollowersCount / 50) + 1));

            var followersUserNames = Followers?.Value.Select(x => x.UserName).ToList();

            var userInfo = new List<UserInfo>();

            foreach (var userName in followersUserNames)
            {
                var user = await _instaApi.UserProcessor.GetUserAsync(userName);
                if (user != null)
                {
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

        public async Task<(Status, List<UnfollowerInfo>)> GetUnfollowersAsync()
        {
            if (_instaApi == null)
                return (new Status(StatusCodes.Status400BadRequest, "You are not logged in"), new List<UnfollowerInfo>());

            if (string.IsNullOrWhiteSpace(CurrentUsername))
                CurrentUsername = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;

            List<UnfollowerInfo>? result = null;

            using (var scope = _scopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                result = await _dbContext.FollowersStatistics
                    .Where(x => x.Username == CurrentUsername)
                    .Select(x => x.UnfollowMe)
                    .FirstOrDefaultAsync();

                result = result.OrderByDescending(x => x.DateOfUnfollowing).ToList();
            }

            return (new Status(), result);
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

        public async Task<(Status, LoggedUserInfoDto)> GetUserInfoAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername))
                CurrentUsername = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;

            using (var scope = _scopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var user = await _dbContext.FollowersStatistics.FirstOrDefaultAsync(x => x.Username == CurrentUsername);
                if (user == null)
                    return (new Status(StatusCodes.Status404NotFound, "User not found"), new LoggedUserInfoDto());

                var userInfo = await _instaApi.UserProcessor.GetUserInfoByUsernameAsync(CurrentUsername);

                FollowersCount = userInfo.Value.FollowerCount;
                FollowingCount = userInfo.Value.FollowingCount;

                return (new Status(), new LoggedUserInfoDto()
                {
                    Username = user.Username,
                    Image = await DownloadImage(user.Image),
                    Followers = FollowersCount,
                    Following = FollowingCount
                });
            }
        }

        public async Task<(Status, List<UnfollowersDto>)> GetPostsStatistic()
        {
            if (string.IsNullOrWhiteSpace(CurrentUsername))
                CurrentUsername = _instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;


            var recentPosts = await _instaApi.UserProcessor.GetUserMediaAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad(5)); //per page 50 => last 250 posts

            var likeCounts = new Dictionary<string, int>();

            foreach (var post in recentPosts.Value)
            {
                var likes = await _instaApi.MediaProcessor.GetMediaLikersAsync(post.InstaIdentifier);

                foreach (var liker in likes.Value)
                {
                    var usernameLiker = liker.UserName;
                    if (likeCounts.ContainsKey(usernameLiker))
                        likeCounts[usernameLiker]++;
                    else
                        likeCounts[usernameLiker] = 1;
                }
            }

            var topLikers = likeCounts.OrderByDescending(pair => pair.Value).ToList();

            var unfollowersDtos = new List<UnfollowersDto>();

            if (Followers == null)
                Followers = await _instaApi.UserProcessor.GetUserFollowersAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad((int)(FollowersCount / 50) + 1));

            if (Following == null)
                Following = await _instaApi.UserProcessor.GetUserFollowingAsync(CurrentUsername, PaginationParameters.MaxPagesToLoad((int)(FollowingCount / 50) + 1));

            var followerUserNames = Followers?.Value?.Select(x => x.UserName).ToList();
            var followingUserNames = Following?.Value?.Select(x => x.UserName).ToList();

            foreach (var liker in topLikers)
            {
                var user = await _instaApi.UserProcessor.GetUserAsync(liker.Key);
                if (user != null)
                {
                    long userId = user.Value.Pk;

                    unfollowersDtos.Add(new UnfollowersDto()
                    {
                        UserId = userId,
                        Image = await DownloadImage(user.Value.ProfilePicture),
                        Username = user.Value.UserName,
                        IFollow = followingUserNames.Any(x => x == user.Value.UserName),
                        FollowMe = followerUserNames.Any(x => x == user.Value.UserName),
                        TotalPostsLiked = liker.Value
                    });
                }
            }

            return (new Status(), unfollowersDtos);
        }

    }
}
