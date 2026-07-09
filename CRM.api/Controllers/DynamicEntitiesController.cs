using Crm.Application.DTOs.DynamicEntities;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicEntitiesController : ControllerBase
    {
        private readonly IDynamicEntityService _dynamicEntityService;

        public DynamicEntitiesController(IDynamicEntityService dynamicEntityService)
        {
            _dynamicEntityService = dynamicEntityService;
        }

        // GET: api/dynamicentities
        [HttpGet]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _dynamicEntityService.GetAllEntitiesAsync();
            return Ok(entities);
        }

        // GET: api/dynamicentities/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _dynamicEntityService.GetEntityByIdAsync(id);
            if (entity == null)
                return NotFound($"Entity with ID {id} not found.");

            return Ok(entity);
        }

        [HttpGet("by-name/{name}")]
        [Authorize(Policy = "RepOrAbove")]
        public async Task<IActionResult> GetByName(string name)
        {
            var entity = await _dynamicEntityService.GetEntityByNameAsync(name);

            if (entity == null)
                return NotFound(new
                {
                    success = false,
                    message = $"Entity with name '{name}' not found."
                });

            return Ok(new
            {
                success = true,
                data = entity
            });
        }



        // POST: api/dynamicentities
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateDynamicEntityDto dto)
        {
            var created = await _dynamicEntityService.CreateEntityAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/dynamicentities/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicEntityDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched entity ID.");

            var updated = await _dynamicEntityService.UpdateEntityAsync(dto);
            return Ok(updated);
        }

        // DELETE: api/dynamicentities/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _dynamicEntityService.DeleteEntityAsync(id);
            if (!success)
                return NotFound($"Entity with ID {id} not found.");

            return Ok(new { message = "Entity deleted successfully." });
        }

        // PUT: api/dynamicentities/restore/{id}
        [HttpPut("restore/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _dynamicEntityService.RestoreEntityAsync(id);
            if (!result)
                return NotFound("Entity not found or already active.");

            return Ok(new { message = "Entity restored successfully." });
        }
    }
}
