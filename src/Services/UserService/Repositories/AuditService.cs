using Common.Entities;
using System.Security.Claims;
using System.Text.Json;
using UserService.Data;
using UserService.Domain.Models;
using UserService.Interfaces;

namespace UserService.Repositories
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogChangesAsync<TEntity>(TEntity oldEntity, TEntity newEntity, string userId) where TEntity : BaseEntity
        {
            try
            {
                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();
                var changedColumns = new List<string>();

                var properties = typeof(TEntity).GetProperties();
                foreach (var prop in properties)
                {
                    var oldValue = prop.GetValue(oldEntity);
                    var newValue = prop.GetValue(newEntity);

                    if (!Equals(oldValue, newValue))
                    {
                        oldValues[prop.Name] = oldValue;
                        newValues[prop.Name] = newValue;
                        changedColumns.Add(prop.Name);
                    }
                }

                var auditLog = new AuditLog
                {
                    TableName = typeof(TEntity).Name,
                    Action = "UPDATE",
                    KeyValues = JsonSerializer.Serialize(new { Id = newEntity.Id }),
                    OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null,
                    AffectedColumns = changedColumns.Any() ? string.Join(",", changedColumns) : null,
                    CreatedBy = userId ?? GetCurrentUserId(),
                    CreatedTime = DateTime.UtcNow
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log changes for {Entity}", typeof(TEntity).Name);
                throw;
            }
        }

        public async Task LogCustomEventAsync(string tableName, string action, string entityId, Dictionary<string, object> additionalData = null)
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                Action = action,
                KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
                OldValues = null,
                NewValues = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                AffectedColumns = null,
                CreatedBy = GetCurrentUserId(),
                CreatedTime = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogUserActivityAsync(string userId, string action, Dictionary<string, object> metadata = null)
        {
            var auditLog = new AuditLog
            {
                TableName = "UserActivities",
                Action = action,
                KeyValues = JsonSerializer.Serialize(new { UserId = userId }),
                OldValues = null,
                NewValues = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                AffectedColumns = null,
                CreatedBy = userId,
                CreatedTime = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogSecurityEventAsync(string userId, string eventType, string ipAddress = null, string userAgent = null, Dictionary<string, object> details = null)
        {
            var securityData = new Dictionary<string, object>
            {
                ["IPAddress"] = ipAddress ?? GetClientIpAddress(),
                ["UserAgent"] = userAgent ?? GetUserAgent()
            };

            if (details != null)
            {
                foreach (var item in details)
                {
                    securityData[item.Key] = item.Value;
                }
            }

            var auditLog = new AuditLog
            {
                TableName = "SecurityEvents",
                Action = eventType,
                KeyValues = JsonSerializer.Serialize(new { UserId = userId }),
                OldValues = null,
                NewValues = JsonSerializer.Serialize(securityData),
                AffectedColumns = null,
                CreatedBy = userId,
                CreatedTime = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogBulkChangesAsync(List<AuditEntry> auditEntries)
        {
            var auditLogs = auditEntries.Select(entry => entry.ToAudit()).ToList();

            // Set common properties
            auditLogs.ForEach(log =>
            {
                log.CreatedBy = log.CreatedBy ?? GetCurrentUserId();
                log.CreatedTime = DateTime.UtcNow;
            });

            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
        }

        #region Helpers
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
        }

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }
        #endregion
    }
}
