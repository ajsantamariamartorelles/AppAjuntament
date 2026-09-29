using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Usuari;

namespace AppAjuntament.Models.Documents;

/// <summary>
/// Document genèric vinculat a qualsevol entitat de l'aplicatiu (patró polimòrfic).
/// EntityType indica el tipus d'entitat (veure DocumentEntityType) i EntityId l'Id concret.
/// El fitxer físic s'emmagatzema a MinIO; PathMinio conté la ruta bucket/objectKey.
/// Mapeig: Documents
/// </summary>
[Table("Documents")]
public class Document
{
    public int Id { get; set; }

    /// <summary>Tipus d'entitat: Voluntari, Arma, Subvencio, Tercer, ... (veure DocumentEntityType)</summary>
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Id de l'entitat referenciada</summary>
    public int EntityId { get; set; }

    /// <summary>FK → Aux_TipusDocument. NULL quan s'usa TipusPersonalitzat</summary>
    public int? TipusDocumentId { get; set; }

    /// <summary>Tipus personalitzat quan s'escull "Altres"</summary>
    [MaxLength(100)]
    public string? TipusPersonalitzat { get; set; }

    [Required, MaxLength(255)]
    public string Nom { get; set; } = string.Empty;

    /// <summary>Ruta a MinIO: bucket/objectKey</summary>
    [Required, MaxLength(500)]
    public string PathMinio { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long? MidaBytes { get; set; }

    public DateOnly? DataDocument { get; set; }

    public string? Observacions { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    // Navegació
    public virtual TipusDocument? TipusDocument { get; set; }
    public virtual Usuari? CreatedByUser { get; set; }
    public virtual Usuari? UpdatedByUser { get; set; }
}
