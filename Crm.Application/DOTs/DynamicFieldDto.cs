namespace Crm.Application.DTOs.DynamicEntities
{
    public class DynamicFieldDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string Label { get; set; } = null!;

        // REQUIRED BY DB
        public string DataType { get; set; } = "string";   // string, int, bool
        public string FieldType { get; set; } = "Text";    // Text, Number, Date
        public string ControlType { get; set; } = "textbox"; // textbox, select

        public bool IsRequired { get; set; } = false;
        public bool IsVisible { get; set; } = true;

        // SAFE DEFAULTS
        public int DisplayOrder { get; set; } = 0;
    }
}
