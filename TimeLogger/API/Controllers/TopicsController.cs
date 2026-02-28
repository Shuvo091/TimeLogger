using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillAllocationTracker.Application.DTOs;
using SkillAllocationTracker.Application.Services;
using System;
using System.Threading.Tasks;

namespace SkillAllocationTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicService _service;

        public TopicsController(ITopicService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _service.GetByIdAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TopicDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] TopicDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}