using Microsoft.AspNetCore.Mvc;
using RiverAPI.Infrastructure.Repositories;

namespace RiverAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiversController : ControllerBase
    {
        private readonly IRiverRepository _repository;
        public RiversController(IRiverRepository repository) { _repository = repository; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (pageSize > 50) pageSize = 50;

            var rivers = await _repository.GetAllAsync(page, pageSize);
            return Ok(rivers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var river = await _repository.GetByIdAsync(id);
            if (river == null) return NotFound();
            return Ok(river);
        }
    }
}