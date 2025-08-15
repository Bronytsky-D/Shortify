using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shottify.Application.Abstraction.Service;
using System.Security.Claims;

namespace Shortify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedirectController : Controller
    {
        private readonly ILinkService _linkService;
        public RedirectController(ILinkService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> RedirectToOriginalUrl(Guid id)
        {

            var linkRespone = await _linkService.GetLinkAsync(id);
            var link = (LinkEntry)linkRespone.Result;
            if (!linkRespone.Success)
            {
                return NotFound();
            }

            return Redirect(link.OriginalUrl);
        }
        //[Authorize]
        //[HttpGet("about/{userId}")]
        //public IActionResult RedirectToAbout(string userid)
        //{
        //    var tokenString = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true, 
        //        SameSite = SameSiteMode.None, 
        //        Expires = DateTimeOffset.UtcNow.AddMinutes(1) 
        //    };

        //    Response.Cookies.Append("X-Access-Token", tokenString, cookieOptions);

        //    var razorBaseUrl = config["RazorBaseUrl"];
        //    var url = $"{razorBaseUrl}/About";
        //    return Redirect(url);
        //}
    }
}
