using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using unfollowers.Services.InstagramService;

namespace unfollowers.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class InstaController : ControllerBase
    {
        private readonly IInstaService _instaService;
        public InstaController(IInstaService instaService)
        {
            _instaService = instaService;
        }

        [HttpGet]
        [ActionName("login")]
        public async Task<IActionResult> LoginUser([FromQuery] string username, [FromQuery] string password)
        {
            var result = await _instaService.Login(username, password);

            if (result.Code == StatusCodes.Status200OK)
                return Ok();

            return StatusCode(result.Code, new { Message = result.ErrorMessage });
        }

        [HttpGet]
        [ActionName("nonfollowers")]
        public async Task<IActionResult> GetNonFollowers()
        {
            var (result, unfollowersDto) = await _instaService.GetNonFollowersDtos();

            if (result.Code == StatusCodes.Status200OK)
                return Ok(unfollowersDto);

            return StatusCode(result.Code, new { Message = result.ErrorMessage });
        }

        [HttpGet]
        [ActionName("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            _instaService.Logout();

            return Ok();
        }

        [HttpGet]
        [ActionName("unfollowers")]
        public async Task<IActionResult> GetUnFollowers()
        {
            var (result, unfollowersDto) = await _instaService.GetUnfollowersAsync();

            if (result.Code == StatusCodes.Status200OK)
                return Ok(unfollowersDto);

            return StatusCode(result.Code, new { Message = result.ErrorMessage });
        }

        [HttpGet]
        [ActionName("userInfo")]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            var (result, userInfo) = await _instaService.GetUserInfoAsync();

            if (result.Code == StatusCodes.Status200OK)
                return Ok(userInfo);

            return StatusCode(result.Code, new { Message = result.ErrorMessage });
        }

        [HttpGet]
        [ActionName("topLikers")]
        public async Task<IActionResult> GetPostsStatistic()
        {
            var (result, likers) = await _instaService.GetPostsStatistic();

            if (result.Code == StatusCodes.Status200OK)
                return Ok(likers);

            return StatusCode(result.Code, new { Message = result.ErrorMessage });
        }
    }
}
