using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortify.Application.DTOs;
using Shortify.Domain.Models;
using Shortify.Infrastructure;
using Shortify.Infrastructure.PostgreSQL;
using Shottify.Application.Abstraction.DTOs;
using Shottify.Application.Abstraction.Service;
using System.Security.Claims;

namespace Shortify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : Controller
    {
        private readonly ILinkService _linkService;
        private readonly ILinkShortenerService _linkShortenerService;
        public LinkController(ILinkService linkService, 
            ILinkShortenerService linkShortenerService)
        {
            _linkService = linkService;
            _linkShortenerService = linkShortenerService;
        }
        
        [HttpGet]
        public async Task<IExecutionResponse> GetLinks()
        {
            return await _linkService.GetLinksAsync();
        }
        [HttpGet("{id}")]
        public async Task<IExecutionResponse> GetLink(Guid id)
        {
            return await _linkService.GetLinkAsync(id);
        }
        [Authorize]
        [HttpPost]
        public async Task<IExecutionResponse> CreateLink(CreateLinkDTO dto)
        {
            var shortCodeResp = await _linkShortenerService.CreateShortLinkAsync(dto.OriginURL);
            if (!shortCodeResp.Success) return shortCodeResp;

            var shortCode = (string)shortCodeResp.Result;
            LinkEntry link = new LinkEntry
            {
                OriginalUrl = dto.OriginURL,
                CreatedBy = dto.CreatedBy,
                Title = dto.Title,
                Description = dto.Description,
                ShortenedUrl = $"https://shortly/{shortCode}"
            };
            return await _linkService.AddLinkAsync(link);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IExecutionResponse> DeleteLink(Guid id)
        {
            var linkResponse = await _linkService.GetLinkAsync(id);
            if (!linkResponse.Success)
                return linkResponse;

            var link = (LinkEntry)linkResponse.Result;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return ExecutionResponse.Failure("User not authenticated");

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && link.CreatedBy != userId)
                return ExecutionResponse.Failure("You are not allowed to delete this link");

            return await _linkService.RemoveLinkAsync(link);
        }
    }
}
