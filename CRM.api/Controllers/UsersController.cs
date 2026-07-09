using Crm.Application.Interfaces;
using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantProvider _tenantProvider;

        public UsersController(UserManager<ApplicationUser> userManager, ITenantProvider tenantProvider)
        {
            _userManager = userManager;
            _tenantProvider = tenantProvider;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = _tenantProvider.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                fullName = user.FullName,
                tenantId = user.TenantId,
                roles
            });
        }

        // For lead assignment dropdown (Admin/Manager only)
        [HttpGet("assignable")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> GetAssignableUsers()
        {
            var tenantId = _tenantProvider.GetTenantId();
            var users = _userManager.Users.Where(u => u.TenantId == tenantId).ToList();

            var result = new List<object>(users.Count);
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new
                {
                    id = u.Id,
                    email = u.Email,
                    fullName = u.FullName,
                    roles
                });
            }

            return Ok(result);
        }
    }
}

