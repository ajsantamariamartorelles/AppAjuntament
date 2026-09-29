using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Tipus d'arxius que es poden pujar
/// </summary>
[Table("PATR_tipus_arxius")]
public class TipusArxiu
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("extensions_permeses")]
    public string? ExtensionsPermeses { get; set; }

    [Column("mida_maxima_mb")]
    public int MidaMaximaMb { get; set; } = 10;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navegació
    public virtual ICollection<ArxiuPatrimoni> Arxius { get; set; } = new List<ArxiuPatrimoni>();

    // Propietats calculades
    [NotMapped]
    public List<string> ExtensionsList => 
        string.IsNullOrEmpty(ExtensionsPermeses) 
            ? new List<string>() 
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(ExtensionsPermeses) ?? new List<string>();
}

/// <summary>
/// Arxius associats al patrimoni (imatges, documents, plànols, etc.)
/// </summary>
[Table("PATR_arxius_patrimoni")]
public class ArxiuPatrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("patrimoni_id")]
    public int? PatrimoniId { get; set; }

    [Column("tipus_arxiu_id")]
    public int TipusArxiuId { get; set; }

    // Informació de l'arxiu
    [Required]
    [MaxLength(255)]
    [Column("nom_original")]
    public string NomOriginal { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("nom_sistema")]
    public string NomArxiu { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("bucket_minio")]
    public string Bucket { get; set; } = "patrimoni-files";

    [Required]
    [MaxLength(500)]
    [Column("path_minio")]
    public string PathMinio { get; set; } = string.Empty;

    // Metadades
    [Column("mida_bytes")]
    public long? MidaBytes { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    [MaxLength(32)]
    [Column("hash_md5")]
    public string? HashMd5 { get; set; }

    // Informació descriptiva
    [MaxLength(200)]
    [Column("titol")]
    public string? Titol { get; set; }

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [MaxLength(100)]
    [Column("autor")]
    public string? Autor { get; set; }

    [Column("data_captura")]
    public DateTime? DataCreacio { get; set; }

    [NotMapped]
    public string? DretsAutor { get; set; }

    // Organització
    [Column("es_principal")]
    public bool EsPrincipal { get; set; } = false;

    [Column("ordre_visualitzacio")]
    public int OrdreVisualitzacio { get; set; } = 0;

    [Column("colleccio_id")]
    public int? ColleccioId { get; set; }

    [Column("localitzacio_id")]
    public int? LocalitzacioId { get; set; }

    [Column("data_fotografia")]
    public DateOnly? DataFotografia { get; set; }

    [Column("visible_public")]
    public bool VisiblePublic { get; set; } = true;

    // Metadades tècniques (per imatges) - columnes no existents a la BD actual, usar migrations per afegir-les
    [NotMapped]
    public int? AmpladaPx { get; set; }

    [NotMapped]
    public int? AlturaPx { get; set; }

    [NotMapped]
    public int? ResolucioApi { get; set; }

    // Control
    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("uploaded_by")]
    public int? UploadedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [ForeignKey("PatrimoniId")]
    public virtual Patrimoni? Patrimoni { get; set; }

    [ForeignKey("TipusArxiuId")]
    public virtual TipusArxiu TipusArxiu { get; set; } = null!;

    [ForeignKey("ColleccioId")]
    public virtual PatrimoniColeccio? Colleccio { get; set; }

    [ForeignKey("LocalitzacioId")]
    public virtual PatrimoniLocalitzacio? Localitzacio { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    // Propietats calculades
    [NotMapped]
    public string MidaFormatada => FormatBytes(MidaBytes ?? 0);

    [NotMapped]
    public bool EsImatge => MimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

    [MaxLength(1000)]
    [Column("url_minio")]
    public string? UrlMinio { get; set; }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
