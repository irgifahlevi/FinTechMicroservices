using Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;
using System.Security.Claims;
using UserService.Domain.Models;

namespace UserService.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AppDbContext> logger) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure BaseEntity properties for all entities that inherit from it
            foreach (var entityType in builder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
            {
                builder.Entity(entityType.ClrType, e =>
                {
                    e.Property(nameof(BaseEntity.CreatedTime))
                        .HasDefaultValueSql("GETUTCDATE()");

                    e.Property(nameof(BaseEntity.LastModifiedTime))
                        .IsConcurrencyToken();
                });
            }

            // Configure UserProfile specifically
            builder.Entity<UserProfile>(e =>
            {
                e.HasIndex(p => p.UserId).IsUnique();
                e.Property(p => p.TaxNumber).HasMaxLength(20);
            });

            // Configure AuditLog
            builder.Entity<AuditLog>(e =>
            {
                e.HasIndex(a => new { a.TableName, a.KeyValues });
                e.Property(a => a.Action).HasMaxLength(20);
            });

            // Configure AppUser relationships
            builder.Entity<AppUser>(e =>
            {
                e.HasOne(a => a.Profile)
                    .WithOne(p => p.User)
                    .HasForeignKey<UserProfile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                SetBaseEntityFields();
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Concurrency violation occurred");
                throw new Exception("Data was modified by another process. Please refresh and try again.", ex);
            }
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            try
            {
                SetBaseEntityFields();
                await AuditChangesAsync(cancellationToken);
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Concurrency violation occurred");
                throw new Exception("Data was modified by another process. Please refresh and try again.", ex);
            }
        }

        private void SetBaseEntityFields()
        {
            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = userId;
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

        private async Task AuditChangesAsync(CancellationToken cancellationToken)
        {
            var auditEntries = OnBeforeSaveChanges();
            if (auditEntries.Count == 0) return;

            try
            {
                await AuditLogs.AddRangeAsync(
                    auditEntries.Select(e => e.ToAudit()),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit logs");
                // Continue without audit if failed
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached &&
                            e.State != EntityState.Unchanged &&
                            e.Entity is not AuditLog))
            {
                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ShortName(),
                    UserId = GetCurrentUserId()
                };

                entries.Add(auditEntry);

                foreach (var property in entry.Properties.Where(p => !p.IsTemporary))
                {
                    var propertyName = property.Metadata.Name;

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

                        case EntityState.Modified when property.IsModified:
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;
                    }
                }
            }

            return entries;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
        }
    }
}
