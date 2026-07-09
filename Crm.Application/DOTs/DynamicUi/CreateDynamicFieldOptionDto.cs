using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicUi
{
    public class CreateDynamicFieldOptionDto
    {
        [Required]
        public Guid FieldId { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        public string Label { get; set; } = string.Empty;

        public int Order { get; set; } = 0;
    }
}
