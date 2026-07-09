namespace Crm.Domain.Entities.Dynamic
{
    public class DynamicPermission : BaseEntity //— Access Control //view/edit
    {
        //public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityId { get; set; }
        public DynamicEntity Entity { get; set; } = null!;
        public string RoleName { get; set; } = string.Empty; // e.g. "Admin", "User"
        public bool CanRead { get; set; } = true;
        public bool CanCreate { get; set; } = true;
        public bool CanUpdate { get; set; } = true;
        public bool CanDelete { get; set; } = false;
    }
}
