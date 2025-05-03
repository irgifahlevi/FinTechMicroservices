using Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using UserService.Domain.Models;

namespace UserService.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor ) : base( options )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProfile>(e =>
            {
                e.Property(b => b.CreatedTime).HasDefaultValueSql("GETUTCDATE()");
                e.Property(b => b.LastModifiedTime).IsConcurrencyToken();
            });

            builder.Entity<AuditLog>(e =>
            {
                e.Property(b => b.CreatedTime).HasDefaultValueSql("GETUTCDATE()");
                e.Property(b => b.LastModifiedTime).IsConcurrencyToken();
            });

            // relationships
            builder.Entity<AppUser>()
                .HasOne(a => a.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetBaseEntityFields();
            await AuditChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetBaseEntityFields()
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var now = DateTime.UtcNow;

            foreach(var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = userId ?? "SYSTEM";
                        entry.Entity.CreatedTime = now;
                        entry.Entity.RowStatus = true;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = userId;
                        entry.Entity.LastModifiedTime = now;
                        break;
                }
            }
        }

        private async Task AuditChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            if(auditEntries.Any())
            {
                await AuditLogs.AddRangeAsync(auditEntries.Select(Q => Q.ToAudit()));
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditEntry>();

            foreach(var entry in ChangeTracker.Entries())
            {
                if(entry.Entity is AuditLog || entry.State == EntityState.Detached ||  entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                { 
                    TableName = entry.Metadata.GetTableName(),
                    UserId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                entries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) 
                        continue;

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
            }

            return entries;
        }
    }
}
