using Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<AuditLog>> GetRecentEntityLogsAsync(string entityName, string entityId, int count = 10)
        {
            try
            {
                return await _context.AuditLogs
                    .Where(a => a.TableName == entityName && a.KeyValues.Contains(entityId))
                    .OrderByDescending(a => a.CreatedTime)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving recent logs for {entityName} with ID {entityId}");
                throw;
            }
        }

        public async Task<List<AuditLog>> GetUserActivityLogsAsync(string userId, int count = 10)
        {
            try
            {
                return await _context.AuditLogs
                    .Where(a => a.CreatedBy == userId)
                    .OrderByDescending(a => a.CreatedTime)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activity logs for user {userId}");
                throw;
            }
        }

        public async Task LogChangesAsync(string entityName, string entityId, object oldValues, object newValues, string userId)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    TableName = entityName,
                    Action = "UPDATE",
                    KeyValues = JsonSerializer.Serialize(new { Id = entityId }),
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    AffectedColumns = null,
                    CreatedBy = string.IsNullOrEmpty(userId) ? GetCurrentUserId() : userId,
                    CreatedTime = DateTime.UtcNow,
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging changes for {entityName} with ID {entityId}");
                throw;
            }
        }

        public async Task LogCustomEventAsync(string tableName, string action, string entityId, Dictionary<string, object> additionalData = null)
        {
            try
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
                    CreatedTime = DateTime.UtcNow,
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Error logging custom event '{action}' for {tableName}, with ID {entityId}");
                throw;
            }
        }

        public async Task LogSecurityEventAsync(string userId, string eventType, string ipAddress = null, string userAgent = null, Dictionary<string, object> details = null)
        {
            try
            {
                userId = string.IsNullOrEmpty(userId) ? GetCurrentUserId() : userId;

                var securityData = new Dictionary<string, object>
                {
                    ["IPAddress"] = ipAddress ?? GetClientIpAddress(),
                    ["UserAgent"] = userAgent ?? GetUserAgent(),
                    ["Timestamp"] = DateTime.UtcNow.ToString("o")
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging security event '{eventType}' for user {userId}");
                throw;
            }
        }

        public async Task LogUserActivityAsync(string userId, string action, Dictionary<string, object> metadata = null)
        {
            try
            {
                userId = string.IsNullOrEmpty(userId) ? GetCurrentUserId() : userId;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging user activity '{action}' for user {userId}");
                throw;
            }
        }

        public List<AuditEntry> ProcessChanges(IEnumerable<EntityEntry> entries, string userId)
        {
            throw new NotImplementedException();
        }

        public Task SaveAuditLogsAsync(List<AuditEntry> auditEntries)
        {
            throw new NotImplementedException();
        }


        #region Helpers
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
        }

        private string GetClientIpAddress()
        {
            if (_httpContextAccessor.HttpContext == null)
                return null;

            var ip = _httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.ToString();

            // Check for forwarded headers (for applications behind proxies)
            if (string.IsNullOrEmpty(ip) && _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            return ip;
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }
        #endregion
    }
}
