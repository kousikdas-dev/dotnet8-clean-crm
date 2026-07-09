using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicFieldValues
{
    public class CreateDynamicFieldValueDto
    {
        [Required]
        public Guid FieldId { get; set; }

        [MaxLength(200)]
        public string FieldName { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}
