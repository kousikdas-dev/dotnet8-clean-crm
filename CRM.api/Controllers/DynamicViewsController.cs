using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class DynamicViewsController : ControllerBase
    {
        private readonly IDynamicViewService _viewService;

        public DynamicViewsController(IDynamicViewService viewService)
        {
            _viewService = viewService;
        }

        // ✅ GET: api/DynamicViews
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var views = await _viewService.GetAllViewsAsync();
            return Ok(views);
        }

        // ✅ GET: api/DynamicViews/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var view = await _viewService.GetViewByIdAsync(id);
            return Ok(view);
        }

        // ✅ GET: api/DynamicViews/entity/{entityId}
        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var views = await _viewService.GetViewsByEntityIdAsync(entityId);
            return Ok(views);
        }

        // ✅ POST: api/DynamicViews
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicViewDto dto)
        {
            var created = await _viewService.CreateViewAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/DynamicViews/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicViewDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched view ID.");

            var updated = await _viewService.UpdateViewAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/DynamicViews/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _viewService.DeleteViewAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ✅ POST: api/DynamicViews/{id}/restore
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await _viewService.RestoreViewAsync(id);
            return success ? Ok(true) : NotFound();
        }
    }
}
