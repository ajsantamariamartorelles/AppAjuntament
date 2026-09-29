using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Log
{
    [Table("logs")]
    public class Log
    {
        [Key]
        public long Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string Level { get; set; } // Debug, Information, Warning, Error, Fatal
        public required string Message { get; set; }
        public string? Exception { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public string? IpAddress { get; set; }
        public string? SessionId { get; set; }
        public string? AdditionalData { get; set; }
    }
}
