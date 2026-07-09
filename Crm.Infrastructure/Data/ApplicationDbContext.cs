using Crm.Domain.Entities;
using Crm.Domain.Entities.Dynamic;
using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ DbSets
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<DynamicEntity> DynamicEntities { get; set; }
        public DbSet<DynamicField> DynamicFields { get; set; }
        public DbSet<DynamicRecord> DynamicRecords { get; set; }
        public DbSet<DynamicFieldValue> DynamicFieldValues { get; set; }

        public DbSet<DynamicFieldOption> DynamicFieldOptions { get; set; }
        public DbSet<DynamicRelationship> DynamicRelationships { get; set; }
        public DbSet<DynamicView> DynamicViews { get; set; }
        public DbSet<DynamicPermission> DynamicPermissions { get; set; }
        public DbSet<LeadAttachment> LeadAttachments { get; set; }
        public DbSet<Tenant> Tenants => Set<Tenant>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Relationship: Entity → Fields (One-to-Many)
            modelBuilder.Entity<DynamicEntity>()
                .HasMany(e => e.Fields)
                .WithOne(f => f.Entity)
                .HasForeignKey(f => f.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Relationship: Entity → Records (One-to-Many)
            modelBuilder.Entity<DynamicEntity>()
                .HasMany(e => e.Records)
                .WithOne(r => r.Entity)
                .HasForeignKey(r => r.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Relationship: Record → FieldValues (One-to-Many)
            modelBuilder.Entity<DynamicRecord>()
                .HasMany(r => r.FieldValues)
                .WithOne(v => v.Record)
                .HasForeignKey(v => v.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // ⚙️ Relationship: Field → FieldValues (One-to-Many) — disable cascade to avoid multiple cascade paths
            modelBuilder.Entity<DynamicField>()
                .HasMany(f => f.FieldValues)
                .WithOne(v => v.Field)
                .HasForeignKey(v => v.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Fix multiple cascade paths for DynamicRelationships
            modelBuilder.Entity<DynamicRelationship>()
                .HasOne(r => r.SourceEntity)
                .WithMany(e => e.RelationshipsFrom)
                .HasForeignKey(r => r.SourceEntityId)
                .OnDelete(DeleteBehavior.Restrict);  // ✅ No cascade

            modelBuilder.Entity<DynamicRelationship>()
                .HasOne(r => r.TargetEntity)
                .WithMany(e => e.RelationshipsTo)
                .HasForeignKey(r => r.TargetEntityId)
                .OnDelete(DeleteBehavior.Restrict);  // ✅ No cascade
        




        // ✅ Soft delete filters
            modelBuilder.Entity<Contact>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<DynamicEntity>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<DynamicField>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<DynamicRecord>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<DynamicFieldValue>().HasQueryFilter(v => !v.IsDeleted);

            
           
        }
    }
}
