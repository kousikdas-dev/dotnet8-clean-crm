using Crm.Application.DTOs.DynamicUi;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class DynamicPermissionsController : ControllerBase
    {
        private readonly IDynamicPermissionService _permissionService;

        public DynamicPermissionsController(IDynamicPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // ✅ GET: api/DynamicPermissions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        // ✅ GET: api/DynamicPermissions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            return Ok(permission);
        }

        // ✅ GET: api/DynamicPermissions/entity/{entityId}
        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var permissions = await _permissionService.GetPermissionsByEntityIdAsync(entityId);
            return Ok(permissions);
        }

        // ✅ POST: api/DynamicPermissions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicPermissionDto dto)
        {
            var created = await _permissionService.CreatePermissionAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/DynamicPermissions/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicPermissionDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched permission ID.");

            var updated = await _permissionService.UpdatePermissionAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/DynamicPermissions/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _permissionService.DeletePermissionAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ✅ POST: api/DynamicPermissions/{id}/restore
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var success = await _permissionService.RestorePermissionAsync(id);
            return success ? Ok(true) : NotFound();
        }
    }
}
