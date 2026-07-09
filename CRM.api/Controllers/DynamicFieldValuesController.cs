using Crm.Application.DTOs.DynamicFieldValues;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DynamicFieldValuesController : ControllerBase
    {
        private readonly IDynamicFieldValueService _dynamicFieldValueService;

        public DynamicFieldValuesController(IDynamicFieldValueService dynamicFieldValueService)
        {
            _dynamicFieldValueService = dynamicFieldValueService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var values = await _dynamicFieldValueService.GetAllValuesAsync();
            return Ok(values);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var value = await _dynamicFieldValueService.GetValueByIdAsync(id);
            return Ok(value);
        }

        [HttpGet("record/{recordId:guid}")]
        public async Task<IActionResult> GetByRecord(Guid recordId)
        {
            var values = await _dynamicFieldValueService.GetValuesByRecordIdAsync(recordId);
            return Ok(values);
        }

        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(Guid entityId)
        {
            var values = await _dynamicFieldValueService.GetValuesByEntityIdAsync(entityId);
            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicFieldValueDto dto)
        {
            var created = await _dynamicFieldValueService.CreateValueAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicFieldValueDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched field value ID.");

            var updated = await _dynamicFieldValueService.UpdateValueAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _dynamicFieldValueService.DeleteValueAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
