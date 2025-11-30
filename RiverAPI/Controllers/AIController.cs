using Microsoft.AspNetCore.Mvc;
using RiverAPI.Services;

namespace RiverAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly SemanticRiverService _semanticService;

        public AIController(SemanticRiverService semanticService)
        {
            _semanticService = semanticService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query cannot be empty.");

            var results = await _semanticService.SearchAsync(query);
            var response = new
            {
                Count = results.Count(),
                Query = query,
                Data = results
            };

            return Ok(response);
        }
    }
}