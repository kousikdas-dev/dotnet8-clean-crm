using System;

namespace Crm.Application.DTOs.DynamicUi
{
    public class DynamicFieldOptionDto
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsDeleted { get; set; }
    }
}
