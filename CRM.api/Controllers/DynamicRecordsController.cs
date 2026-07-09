using Crm.Application.DTOs.DynamicRecords;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicRecordsController : ControllerBase
    {
        private readonly IDynamicRecordService _dynamicRecordService;
        private readonly IDynamicEntityService _dynamicEntityService;

        public DynamicRecordsController(IDynamicRecordService dynamicRecordService, IDynamicEntityService dynamicEntityService)
        {
            _dynamicRecordService = dynamicRecordService;
            _dynamicEntityService = dynamicEntityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _dynamicRecordService.GetAllRecordsAsync();
            return Ok(records);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _dynamicRecordService.GetRecordByIdAsync(id);
            return Ok(record);
        }

        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var records = await _dynamicRecordService.GetRecordsByEntityIdAsync(entityId);
            return Ok(records);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicRecordDto dto)
        {
            var created = await _dynamicRecordService.CreateRecordAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicRecordDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched record ID.");

            // Lead assignment rule: only Admin/Manager can change lead owner
            var entity = await _dynamicEntityService.GetEntityByIdAsync(dto.EntityId);
            if (entity?.Name == "Lead")
            {
                var existing = await _dynamicRecordService.GetRecordByIdAsync(dto.Id);
                if (existing == null) return NotFound("Dynamic Record not found.");

                var oldOwner = TryGetJsonString(existing.DataJson, "leadOwner") ?? TryGetJsonString(existing.DataJson, "owner") ?? "";
                var newOwner = TryGetJsonString(dto.DataJson, "leadOwner") ?? TryGetJsonString(dto.DataJson, "owner") ?? "";

                if (!string.Equals(oldOwner?.Trim(), newOwner?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    if (!(User.IsInRole("Admin") || User.IsInRole("Manager")))
                        return Forbid();
                }
            }

            var updated = await _dynamicRecordService.UpdateRecordAsync(dto);
            return Ok(updated);
        }

        private static string? TryGetJsonString(string json, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                if (!doc.RootElement.TryGetProperty(propertyName, out var prop)) return null;
                return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
            }
            catch
            {
                return null;
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _dynamicRecordService.DeleteRecordAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpPut("restore/{id:guid}")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _dynamicRecordService.RestoreRecordAsync(id);
            return Ok(result);
        }

        // Merge duplicates (Lead only) - Manager/Admin
        [HttpPost("leads/merge")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> MergeLead([FromBody] MergeLeadDto dto)
        {
            if (dto.TargetLeadId == dto.SourceLeadId) return BadRequest("Target and Source must be different.");
            var ok = await _dynamicRecordService.MergeLeadAsync(dto.TargetLeadId, dto.SourceLeadId);
            return ok ? Ok(true) : BadRequest("Unable to merge leads.");
        }

        public class MergeLeadDto
        {
            public Guid TargetLeadId { get; set; }
            public Guid SourceLeadId { get; set; }
        }
    }
}
