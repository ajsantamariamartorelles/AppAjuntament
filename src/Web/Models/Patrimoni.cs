
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Model principal del patrimoni municipal
/// </summary>
[Table("PATR_patrimonis")]
public class Patrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Identificació
    [Required]
    [MaxLength(50)]
    [Column("codi_inventari")]
    public string CodiInventari { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("nom_alternatiu")]
    public string? NomAlternatiu { get; set; }

    // Classificació
    [Required(ErrorMessage = "El tipus de patrimoni és obligatori")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioneu un tipus de patrimoni")]
    [Column("tipus_patrimoni_id")]
    public int TipusPatrimoniId { get; set; }

    [MaxLength(100)]
    [Column("subtipus")]
    public string? Subtipus { get; set; }

    // Descripció
    [Column("descripcio_breu")]
    public string? DescripcioBreu { get; set; }

    [Column("descripcio_detallada")]
    public string? DescripcioDetallada { get; set; }

    [Column("historia")]
    public string? Historia { get; set; }

    // Ubicació
    [Column("ubicacio_id")]
    public int? UbicacioId { get; set; }

    [MaxLength(200)]
    [Column("municipi")]
    public string? Municipi { get; set; }

    [MaxLength(100)]
    [Column("comarca")]
    public string? Comarca { get; set; }

    [MaxLength(100)]
    [Column("provincia")]
    public string? Provincia { get; set; }

    [MaxLength(500)]
    [Column("adreca_completa")]
    public string? AdrecaCompleta { get; set; }

    [MaxLength(20)]
    [Column("numero_policia")]
    public string? NumeroPolicia { get; set; }

    [Column("coordenades")]
    public string? Coordenades { get; set; }

    [Column("latitud")]
    public double? Latitud { get; set; }

    [Column("longitud")]
    public double? Longitud { get; set; }

    [Column("utm_x")]
    public double? UtmX { get; set; }

    [Column("utm_y")]
    public double? UtmY { get; set; }

    // Característiques físiques
    [Column("any_construccio")]
    public int? AnyConstruccio { get; set; }

    [MaxLength(100)]
    [Column("periode_historic")]
    public string? PeriodeHistoric { get; set; }

    [MaxLength(100)]
    [Column("estil_arquitectonic")]
    public string? EstilArquitectonic { get; set; }

    [Column("materials_principals")]
    public string? MaterialsPrincipals { get; set; }

    // Estat i conservació
    [Column("estat_conservacio_id")]
    public int? EstatConservacioId { get; set; }

    [Column("observacions_estat")]
    public string? ObservacionsEstat { get; set; }

    [Column("data_ultima_inspeccio")]
    public DateTime? DataUltimaInspeccio { get; set; }

    // Protecció legal
    [Column("proteccio_legal")]
    public ProteccioLegal ProteccioLegal { get; set; } = ProteccioLegal.Cap;

    [MaxLength(100)]
    [Column("numero_expedient")]
    public string? NumeroExpedient { get; set; }

    [Column("data_declaracio")]
    public DateTime? DataDeclaracio { get; set; }

    [MaxLength(200)]
    [Column("organisme_proteccio")]
    public string? OrganismeProteccio { get; set; }

    // Ús i propietat
    [MaxLength(100)]
    [Column("us_actual")]
    public string? UsActual { get; set; }

    [MaxLength(100)]
    [Column("us_original")]
    public string? UsOriginal { get; set; }

    [MaxLength(200)]
    [Column("propietari_actual")]
    public string? PropietariActual { get; set; }

    [Column("regim_propietat")]
    public RegimPropietat RegimPropietat { get; set; } = RegimPropietat.Desconegut;

    // Valoració
    [Range(1, 5)]
    [Column("valor_historic")]
    public int ValorHistoric { get; set; } = 1;

    [Range(1, 5)]
    [Column("valor_artistic")]
    public int ValorArtistic { get; set; } = 1;

    [Range(1, 5)]
    [Column("valor_arquitectonic")]
    public int ValorArquitectonic { get; set; } = 1;

    [Range(1, 5)]
    [Column("valor_social")]
    public int ValorSocial { get; set; } = 1;

    [Column("valor_total")]
    public decimal ValorTotal { get; set; } = 1.00m;

    // Accés públic
    [Column("accessible_public")]
    public bool AccessiblePublic { get; set; } = false;

    [Column("horari_visites")]
    public string? HorariVisites { get; set; }

    [Column("preu_entrada")]
    public decimal? PreuEntrada { get; set; }

    // Metadades
    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("notes_internes")]
    public string? NotesInternes { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [ForeignKey("TipusPatrimoniId")]
    public virtual TipusPatrimoni TipusPatrimoni { get; set; } = null!;

    [ForeignKey("EstatConservacioId")]
    public virtual EstatConservacio? EstatConservacio { get; set; }

    [ForeignKey("UbicacioId")]
    public virtual Ubicacio? Ubicacio { get; set; }

    public virtual ICollection<ArxiuPatrimoni> Arxius { get; set; } = new List<ArxiuPatrimoni>();
    public virtual ICollection<NotaPatrimoni> NotesPatrimoni { get; set; } = new List<NotaPatrimoni>();
    public virtual ICollection<Inspeccio> Inspeccions { get; set; } = new List<Inspeccio>();
    public virtual ICollection<Intervencio> Intervencions { get; set; } = new List<Intervencio>();
    public virtual ICollection<PatrimoniIdentificador> Identificadors { get; set; } = new List<PatrimoniIdentificador>();

    // Propietats calculades
    [NotMapped]
    public string NomComplet => string.IsNullOrEmpty(NomAlternatiu) ? Nom : $"{Nom} ({NomAlternatiu})";

    [NotMapped]
    public string ColorEstat => EstatConservacio?.Color ?? "#6C757D";

    [NotMapped]
    public string IconaTipus => TipusPatrimoni?.Icona ?? "fa-building";
}

/// <summary>
/// Nivells de protecció legal
/// </summary>
public enum ProteccioLegal
{
    Cap,
    Local,
    Autonomica,
    Nacional,
    Internacional
}

/// <summary>
/// Règim de propietat
/// </summary>
public enum RegimPropietat
{
    [EnumMember(Value = "públic")]
    Public,
    [EnumMember(Value = "privat")]
    Privat,
    [EnumMember(Value = "mixte")]
    Mixte,
    [EnumMember(Value = "desconegut")]
    Desconegut
}
