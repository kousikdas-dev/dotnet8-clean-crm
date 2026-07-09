using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicUi
{
    public class CreateDynamicViewDto
    {
        [Required] public Guid EntityId { get; set; }
        [Required] public string ViewType { get; set; } = "Form"; // Form, Grid
        [Required] public string Name { get; set; } = string.Empty;
        public string? LayoutJson { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateDynamicViewDto : CreateDynamicViewDto
    {
        [Required] public Guid Id { get; set; }
    }

    public class DynamicViewDto : UpdateDynamicViewDto
    {
        public bool IsDeleted { get; set; }
    }
}
