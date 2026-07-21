using AutoMapper; // ✅ Added for AutoMapper
using Crm.Api.Middleware;
using Crm.Application.Interfaces;
using Crm.Application.Services;
using Crm.Application.Validators;
using Crm.Infrastructure.Data;
using Crm.Infrastructure.Identity;
using Crm.Infrastructure.Repositories;
using Crm.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Controllers
// ---------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Enforce consistent JSON casing & ignore nulls
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // ✅ Prevent EF navigation cycles from crashing JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ---------------------------
// CORS Configuration
// ---------------------------
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins", policy =>
//    {
//        policy
//            .WithOrigins(
//                "http://localhost:5173",   // ✅ Vite frontend
//                "http://localhost:3000",   // optional
//                "https://your-frontend-domain.com"
//            )

//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});



// ✅ FluentValidation (modern configuration)
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// ✅ Register validators from your Application layer (where validators live)
builder.Services.AddValidatorsFromAssembly(Assembly.Load("Crm.Application"));

// ---------------------------
// Database Configuration
// ---------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Crm.Infrastructure")
    );

    // ✅ ENABLE DETAILED EF ERRORS (DEV ONLY)
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});


// ---------------------------
// HttpContext & Tenant Services
// ---------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

// ---------------------------
// Repositories & Application Services
// ---------------------------
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// ✅ Dynamic Fields
builder.Services.AddScoped<IDynamicFieldRepository, DynamicFieldRepository>();
builder.Services.AddScoped<IDynamicFieldService, DynamicFieldService>();

// ✅ Dynamic Entities
builder.Services.AddScoped<IDynamicEntityRepository, DynamicEntityRepository>();
builder.Services.AddScoped<IDynamicEntityService, DynamicEntityService>();

// ✅ Dynamic Field Values (NEW)
builder.Services.AddScoped<IDynamicFieldValueRepository, DynamicFieldValueRepository>();
builder.Services.AddScoped<IDynamicFieldValueService, DynamicFieldValueService>();

// ✅ Dynamic Field Values (NEW)
builder.Services.AddScoped<IDynamicRecordRepository, DynamicRecordRepository>();
builder.Services.AddScoped<IDynamicRecordService, DynamicRecordService>();

// ✅ Dynamic Field Options
builder.Services.AddScoped<IDynamicFieldOptionRepository, DynamicFieldOptionRepository>();
builder.Services.AddScoped<IDynamicFieldOptionService, DynamicFieldOptionService>();

// ✅ Dynamic Permissions
builder.Services.AddScoped<IDynamicPermissionRepository, DynamicPermissionRepository>();
builder.Services.AddScoped<IDynamicPermissionService, DynamicPermissionService>();

// ✅ Dynamic Relationships
builder.Services.AddScoped<IDynamicRelationshipRepository, DynamicRelationshipRepository>();
builder.Services.AddScoped<IDynamicRelationshipService, DynamicRelationshipService>();

// ✅ Dynamic Views
builder.Services.AddScoped<IDynamicViewRepository, DynamicViewRepository>();
builder.Services.AddScoped<IDynamicViewService, DynamicViewService>();


// ---------------------------
// Identity Configuration
// ---------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ---------------------------
// JWT Authentication
// ---------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // optional for dev
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)
        )
    };
});

// ---------------------------
// Authorization Policies
// ---------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("RepOrAbove", policy => policy.RequireRole("Admin", "Manager", "Rep"));
});

// ---------------------------
// ✅ AutoMapper Configuration
// ---------------------------
// ---------------------------
// AutoMapper Configuration ✅
// ---------------------------
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<Crm.Application.Mappings.MappingProfile>();
});



// ---------------------------
// Swagger / OpenAPI
// ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1"
    });

    // 🔒 JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer eyJhbGciOi..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 🏷 Tenant header
    c.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Tenant Identifier (e.g. tenant_1, tenant_2)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Tenant"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<ITenantService, TenantService>();


// ---------------------------
// Build & Run the App
// ---------------------------
var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Static files for attachments (wwwroot/uploads/...)
app.UseStaticFiles();

// 1️⃣ Exception Handling (includes FluentValidation)
app.UseExceptionHandling();

// 2️⃣ Global Response Wrapper
app.UseResponseWrapping();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

// 3️⃣ CORS
app.UseCors("AllowFrontend");

// 4️⃣ Authentication / Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5️⃣ Controllers
app.MapControllers();

// ---------------------------
// Run
// ---------------------------
app.Run();
