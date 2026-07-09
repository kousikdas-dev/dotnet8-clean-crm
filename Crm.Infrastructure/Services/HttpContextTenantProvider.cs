using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Crm.Application.Interfaces;

namespace Crm.Infrastructure.Services
{
    public class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HttpContextTenantProvider> _logger;

        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor, ILogger<HttpContextTenantProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        //public string GetTenantId()
        //{
        //    var httpContext = _httpContextAccessor.HttpContext;
        //    var user = httpContext?.User;

        //    var tenantId = user?.FindFirst("tenantId")?.Value
        //        ?? httpContext?.Request.Headers["X-Tenant-Id"].ToString()
        //        ?? "default-tenant";

        //    _logger.LogDebug("Resolved TenantId: {TenantId}", tenantId);

        //    return tenantId;
        //}

        public string GetTenantId()
        {
            var tenantId = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("tenantId")?
                .Value;

            if (string.IsNullOrWhiteSpace(tenantId))
                throw new UnauthorizedAccessException("Tenant not resolved");

            return tenantId;
        }


        public string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            _logger.LogDebug("Resolved UserId: {UserId}", userId);

            return userId;
        }
    }
}
