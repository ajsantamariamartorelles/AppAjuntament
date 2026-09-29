using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

[Table("Tags")]
public class Tag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(7)]
    [Column("color")]
    public string Color { get; set; } = "#6C757D";

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    public virtual ICollection<ArxiuPatrimoni> Arxius { get; set; } = new List<ArxiuPatrimoni>();
}
