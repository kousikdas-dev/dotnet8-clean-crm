using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class DynamicRelationshipsController : ControllerBase
    {
        private readonly IDynamicRelationshipService _relationshipService;

        public DynamicRelationshipsController(IDynamicRelationshipService relationshipService)
        {
            _relationshipService = relationshipService;
        }

        // ✅ GET: api/DynamicRelationships
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var relations = await _relationshipService.GetAllRelationshipsAsync();
            return Ok(relations);
        }

        // ✅ GET: api/DynamicRelationships/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var relation = await _relationshipService.GetRelationshipByIdAsync(id);
            return Ok(relation);
        }

        // ✅ GET: api/DynamicRelationships/entity/{entityId}
        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var relations = await _relationshipService.GetRelationshipsByEntityIdAsync(entityId);
            return Ok(relations);
        }

        // ✅ POST: api/DynamicRelationships
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicRelationshipDto dto)
        {
            var created = await _relationshipService.CreateRelationshipAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/DynamicRelationships/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicRelationshipDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched relationship ID.");

            var updated = await _relationshipService.UpdateRelationshipAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/DynamicRelationships/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _relationshipService.DeleteRelationshipAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ✅ POST: api/DynamicRelationships/{id}/restore
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await _relationshipService.RestoreRelationshipAsync(id);
            return success ? Ok(true) : NotFound();
        }
    }
}
