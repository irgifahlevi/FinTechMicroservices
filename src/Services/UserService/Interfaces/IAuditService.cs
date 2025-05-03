using Common.Entities;
using UserService.Domain.Models;

namespace UserService.Interfaces
{
    public interface IAuditService
    {
        Task LogChangesAsync<TEntity>(TEntity oldEntity, TEntity newEntity, string userId) where TEntity : BaseEntity;
        Task LogCustomEventAsync(string tableName, string action, string entityId, Dictionary<string, object> additionalData = null);
        Task LogUserActivityAsync(string userId, string action, Dictionary<string, object> metadata = null);
        Task LogSecurityEventAsync(string userId, string eventType, string ipAddress = null, string userAgent = null, Dictionary<string, object> details = null);
        Task LogBulkChangesAsync(List<AuditEntry> auditEntries);
    }
}
