using Crm.Application.Interfaces;
using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITenantService _tenantService;


        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ITenantService tenantService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tenantService = tenantService;
        }

        // ============================
        // ✅ REGISTER
        // ============================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return Conflict("User already exists.");

            var tenant = await _tenantService.CreateTenantAsync(model.CompanyName);

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                TenantId = tenant.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Ensure baseline roles exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await _roleManager.RoleExistsAsync("Manager"))
                await _roleManager.CreateAsync(new IdentityRole("Manager"));
            if (!await _roleManager.RoleExistsAsync("Rep"))
                await _roleManager.CreateAsync(new IdentityRole("Rep"));

            // First user in a tenant becomes Admin, others default to Rep
            var hasOtherTenantUsers = await _userManager.Users.AnyAsync(u => u.TenantId == tenant.Id && u.Id != user.Id);
            var assignedRole = hasOtherTenantUsers ? "Rep" : "Admin";
            await _userManager.AddToRoleAsync(user, assignedRole);
            user.RoleType = assignedRole;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User registered successfully.", tenantId = tenant.Id });
        }

        // ============================
        // ✅ LOGIN
        // ============================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password.");

            // Backfill roles for users created before role enforcement existed
            // If the user has no Identity roles, assign one based on RoleType (fallback: Rep).
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!await _roleManager.RoleExistsAsync("Manager"))
                    await _roleManager.CreateAsync(new IdentityRole("Manager"));
                if (!await _roleManager.RoleExistsAsync("Rep"))
                    await _roleManager.CreateAsync(new IdentityRole("Rep"));

                var desired =
                    string.Equals(user.RoleType, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" :
                    string.Equals(user.RoleType, "Manager", StringComparison.OrdinalIgnoreCase) ? "Manager" :
                    string.Equals(user.RoleType, "Rep", StringComparison.OrdinalIgnoreCase) ? "Rep" :
                    "Rep";

                await _userManager.AddToRoleAsync(user, desired);
                user.RoleType = desired;
                await _userManager.UpdateAsync(user);
            }

            var accessToken = await GenerateJwtTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();

            // 💾 Optionally store refresh token in user record
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = accessToken,
                refreshToken,
                user = new { user.FullName, user.Email }
            });
        }

        // ============================
        // ✅ REFRESH TOKEN
        // ============================
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null)
                return Unauthorized("Invalid access token or refresh token.");

            var email = principal.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(email!);
            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid refresh token.");

            var newAccessToken = await GenerateJwtTokenAsync(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        // ============================
        // ✅ LOGOUT
        // ============================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(email!);
            if (user == null)
                return NotFound("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User logged out successfully." });
        }

        // ============================
        // ✅ FORGOT PASSWORD
        // ============================
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new { message = "If the email exists, password reset instructions have been sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: send via email (you can inject IEmailSender)
            var resetLink = $"https://your-frontend-domain.com/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";

            // For dev, return the link directly
            return Ok(new { message = "Password reset link generated.", resetLink });
        }

        // ============================
        // ✅ RESET PASSWORD
        // ============================
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password reset successfully." });
        }

        // ============================
        // 🔐 TOKEN GENERATION
        // ============================
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("tenantId", user.TenantId ?? "default-tenant"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Include Identity roles as role claims so [Authorize(Roles="...")] and policies work
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔍 Extract user from expired token
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!)),
                ValidateLifetime = false // important for refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }

    // ============================
    // 📦 DTOs
    // ============================
    public class RegisterModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string CompanyName { get; set; } = null!; // 👈 tenant name
    }

    public class LoginModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RefreshTokenModel
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordModel
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
