using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Application.DTOs.DynamicUi
{
    public class UpdateDynamicFieldOptionDto : CreateDynamicFieldOptionDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
