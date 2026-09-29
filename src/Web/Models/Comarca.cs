using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Comarques de Catalunya
/// </summary>
// [Table("COMARQUES")]
// public class Comarca
// {
//     [Key]
//     [Column("id")]
//     public int Id { get; set; }

//     [Required]
//     [MaxLength(100)]
//     [Column("nom")]
//     public string Nom { get; set; } = string.Empty;

//     [MaxLength(10)]
//     [Column("codi_ine")]
//     public string? CodiIne { get; set; }

//     [Column("actiu")]
//     public bool Actiu { get; set; } = true;

//     [Column("created_at")]
//     public DateTime CreatedAt { get; set; } = DateTime.Now;

//     [Column("updated_at")]
//     public DateTime UpdatedAt { get; set; } = DateTime.Now;

//     // Navegació
//     public virtual ICollection<Ubicacio> Ubicacions { get; set; } = new List<Ubicacio>();
// }
