using Crm.Application.DTOs.DynamicEntities;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicFieldsController : ControllerBase
    {
        private readonly IDynamicFieldService _dynamicFieldService;

        public DynamicFieldsController(IDynamicFieldService dynamicFieldService)
        {
            _dynamicFieldService = dynamicFieldService;
        }

        // ✅ GET: api/dynamicfields
        [HttpGet]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetAll()
        {
            var fields = await _dynamicFieldService.GetAllFieldsAsync();
            return Ok(fields);
        }

        // ✅ GET: api/dynamicfields/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var field = await _dynamicFieldService.GetFieldByIdAsync(id);
            return Ok(field);
        }

        // ✅ GET: api/dynamicfields/entity/{entityId}
        [HttpGet("entity/{entityId:guid}")]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetByEntityId(Guid entityId)
        {
            var fields = await _dynamicFieldService.GetFieldsByEntityIdAsync(entityId);
            return Ok(fields);
        }

        // ✅ POST: api/dynamicfields
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateDynamicFieldDto dto)
        {
            // This DTO does not carry an EntityId; use the entity-specific route instead.
            return BadRequest("Please use POST /api/DynamicFields/entity/{entityId} to create a field for an entity.");
        }

        // ✅ POST: api/dynamicfields/entity/{entityId}
        [HttpPost("entity/{entityId:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateForEntity(Guid entityId, [FromBody] CreateDynamicFieldDto dto)
        {
            var created = await _dynamicFieldService.CreateFieldForEntityAsync(entityId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/dynamicfields/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicFieldDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched field ID.");

            var updated = await _dynamicFieldService.UpdateFieldAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/dynamicfields/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _dynamicFieldService.DeleteFieldAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ✅ PUT: api/dynamicfields/restore/{id}
        [HttpPut("restore/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _dynamicFieldService.RestoreFieldAsync(id);
            return Ok(result);
        }
    }
}
