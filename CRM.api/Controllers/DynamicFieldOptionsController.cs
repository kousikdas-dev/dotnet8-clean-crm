using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class DynamicFieldOptionsController : ControllerBase
    {
        private readonly IDynamicFieldOptionService _dynamicFieldOptionService;

        public DynamicFieldOptionsController(IDynamicFieldOptionService dynamicFieldOptionService)
        {
            _dynamicFieldOptionService = dynamicFieldOptionService;
        }

        // ✅ GET: api/DynamicFieldOptions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var options = await _dynamicFieldOptionService.GetAllFieldOptionsAsync();
            return Ok(options);
        }

        // ✅ GET: api/DynamicFieldOptions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var option = await _dynamicFieldOptionService.GetFieldOptionsByIdAsync(id);
            return Ok(option);
        }

        // ✅ GET: api/DynamicFieldOptions/entity/{entityId}
        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var options = await _dynamicFieldOptionService.GetFieldOptionsByEntityIdAsync(entityId);
            return Ok(options);
        }

        // ✅ POST: api/DynamicFieldOptions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicFieldOptionDto dto)
        {
            var created = await _dynamicFieldOptionService.CreateFieldOptionsAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/DynamicFieldOptions/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicFieldOptionDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched option ID.");

            var updated = await _dynamicFieldOptionService.UpdateFieldOptionsAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/DynamicFieldOptions/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _dynamicFieldOptionService.DeleteFieldOptionsAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ✅ POST: api/DynamicFieldOptions/{id}/restore
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await _dynamicFieldOptionService.RestoreFieldOptionsAsync(id);
            return success ? Ok(true) : NotFound();
        }
    }
}
