using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.IngressosExterns;

[Table("ingressos_externs_fons_cooperacio")]
public class IngressosExternsFonsCooperacio
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required, MaxLength(30), Column("font")]
    public string Font { get; set; } = "FCLC";

    [Required, MaxLength(10), Column("codi_ine6")]
    public string CodiIne6 { get; set; } = string.Empty;

    [Required, MaxLength(255), Column("municipi")]
    public string Municipi { get; set; } = string.Empty;

    [MaxLength(255), Column("comarca")]
    public string? Comarca { get; set; }

    [Column("exercici")]
    public int Exercici { get; set; }

    [Column("import", TypeName = "decimal(18,2)")]
    public decimal Import { get; set; }

    [Column("import_complementari", TypeName = "decimal(18,2)")]
    public decimal? ImportComplementari { get; set; }

    [Column("poblacio")]
    public int? Poblacio { get; set; }

    [Column("sincronitzat_utc")]
    public DateTime SincronitzatUtc { get; set; }
}
