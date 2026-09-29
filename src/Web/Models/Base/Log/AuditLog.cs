using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Log
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }
        public required string TableName { get; set; }
        public required string RecordId { get; set; }
        public required string Action { get; set; } // INSERT, UPDATE, DELETE
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? SessionId { get; set; }
    }
}
