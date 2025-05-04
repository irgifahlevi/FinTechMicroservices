using Common.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace UserService.Domain.Models
{
    public class AuditLog : BaseEntity
    {
        public string TableName { get; set; }
        public string Action {  get; set; }
        public string KeyValues { get; set; }
        public string? OldValues { get; set; }
        public string NewValues { get; set; }
        public string? AffectedColumns { get; set; }
    }


    // helper class
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            this.Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<string> ChangedColumns { get; } = new();

        public AuditLog ToAudit()
        {
            return new AuditLog
            {
                TableName = TableName,
                Action = Entry.State.ToString(),
                KeyValues = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : null,
                NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : null,
                AffectedColumns = ChangedColumns.Any() ? string.Join(",", ChangedColumns) : null,
                CreatedBy = UserId,
                RowStatus = true
            };
        }
    }
}
