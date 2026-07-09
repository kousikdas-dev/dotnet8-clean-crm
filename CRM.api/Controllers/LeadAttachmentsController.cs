using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeadAttachmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public LeadAttachmentsController(ApplicationDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        [HttpGet("lead/{leadRecordId:guid}")]
        public async Task<IActionResult> List(Guid leadRecordId)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var items = await _db.LeadAttachments
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.LeadRecordId == leadRecordId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    id = a.Id,
                    leadRecordId = a.LeadRecordId,
                    fileName = a.OriginalFileName,
                    contentType = a.ContentType,
                    sizeBytes = a.SizeBytes,
                    createdAt = a.CreatedAt,
                    url = "/" + a.StoragePath.Replace("\\", "/").TrimStart('/')
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("lead/{leadRecordId:guid}")]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> Upload(Guid leadRecordId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is required.");

            var tenantId = _tenantProvider.GetTenantId();
            var userId = _tenantProvider.GetUserId();

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", tenantId, leadRecordId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var safeName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(safeName);
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var storedPath = Path.Combine(uploadsRoot, storedName);

            await using (var stream = System.IO.File.Create(storedPath))
            {
                await file.CopyToAsync(stream);
            }

            var relPath = Path.Combine("uploads", tenantId, leadRecordId.ToString(), storedName);

            var attachment = new LeadAttachment
            {
                TenantId = tenantId,
                CreatedByUserId = userId,
                LeadRecordId = leadRecordId,
                OriginalFileName = safeName,
                ContentType = file.ContentType ?? "application/octet-stream",
                SizeBytes = file.Length,
                StoragePath = relPath
            };

            _db.LeadAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = attachment.Id,
                url = "/" + relPath.Replace("\\", "/"),
                fileName = attachment.OriginalFileName
            });
        }
    }
}

