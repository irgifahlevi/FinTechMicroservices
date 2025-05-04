using Common.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UserService.Domain.Models;

namespace UserService.Interfaces
{
    public interface IAuditService
    {
        /// <summary>
        /// Logs entity changes manually
        /// </summary>
        /// <param name="entityName">Name of the entity/table</param>
        /// <param name="entityId">Primary key of the entity</param>
        /// <param name="oldValues">Old values before change</param>
        /// <param name="newValues">New values after change</param>
        /// <param name="userId">ID of the user making the change</param>
        Task LogChangesAsync(string entityName, string entityId, object oldValues, object newValues, string userId);

        /// <summary>
        /// Logs a custom event to the audit trail
        /// </summary>
        /// <param name="tableName">Name of the table or context</param>
        /// <param name="action">Type of action performed</param>
        /// <param name="entityId">ID of the affected entity</param>
        /// <param name="additionalData">Additional data to log</param>
        Task LogCustomEventAsync(string tableName, string action, string entityId, Dictionary<string, object> additionalData = null);

        /// <summary>
        /// Logs user activity such as login, logout, page access, etc.
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="action">Action performed</param>
        /// <param name="metadata">Additional metadata about the activity</param>
        Task LogUserActivityAsync(string userId, string action, Dictionary<string, object> metadata = null);

        /// <summary>
        /// Logs security-related events like login attempts, password changes, etc.
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="eventType">Type of security event</param>
        /// <param name="ipAddress">IP address (optional)</param>
        /// <param name="userAgent">User agent (optional)</param>
        /// <param name="details">Additional details</param>
        Task LogSecurityEventAsync(string userId, string eventType, string ipAddress = null, string userAgent = null, Dictionary<string, object> details = null);

        /// <summary>
        /// Process tracked entity changes from DbContext to create audit entries
        /// </summary>
        /// <param name="entries">Changed entity entries from DbContext</param>
        /// <param name="userId">ID of the user making changes</param>
        /// <returns>List of audit entries ready to be saved</returns>
        List<AuditEntry> ProcessChanges(IEnumerable<EntityEntry> entries, string userId);

        /// <summary>
        /// Save audit entries to the database
        /// </summary>
        /// <param name="auditEntries">Prepared audit entries</param>
        Task SaveAuditLogsAsync(List<AuditEntry> auditEntries);

        /// <summary>
        /// Gets recent audit logs for a specific entity
        /// </summary>
        /// <param name="entityName">Name of the entity/table</param>
        /// <param name="entityId">ID of the entity</param>
        /// <param name="count">Number of records to return</param>
        Task<List<AuditLog>> GetRecentEntityLogsAsync(string entityName, string entityId, int count = 10);

        /// <summary>
        /// Gets audit logs for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="count">Number of records to return</param>
        Task<List<AuditLog>> GetUserActivityLogsAsync(string userId, int count = 10);
    }
}
