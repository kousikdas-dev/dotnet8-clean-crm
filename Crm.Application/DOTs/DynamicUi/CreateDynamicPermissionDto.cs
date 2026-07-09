using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicUi
{
    public class CreateDynamicPermissionDto
    {
        [Required] public Guid EntityId { get; set; }
        [Required] public string RoleName { get; set; } = string.Empty;
        public bool CanRead { get; set; } = true;
        public bool CanCreate { get; set; } = false;
        public bool CanUpdate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }

    public class UpdateDynamicPermissionDto : CreateDynamicPermissionDto
    {
        [Required] public Guid Id { get; set; }
    }

    public class DynamicPermissionDto : UpdateDynamicPermissionDto
    {
        public bool IsDeleted { get; set; }
    }
}
