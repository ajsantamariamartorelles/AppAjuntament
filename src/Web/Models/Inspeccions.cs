using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Inspeccions del patrimoni
/// </summary>
[Table("PATR_inspeccions")]
public class Inspeccio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("patrimoni_id")]
    public int PatrimoniId { get; set; }

    // Informació de la inspecció
    [Required]
    [Column("data_inspeccio")]
    public DateTime DataInspeccio { get; set; }

    [Column("tipus_inspeccio")]
    public TipusInspeccio TipusInspeccio { get; set; } = TipusInspeccio.Rutinària;

    // Resultats
    [Range(1, 5)]
    [Column("estat_general")]
    public int EstatGeneral { get; set; }

    [Range(1, 5)]
    [Column("estat_estructura")]
    public int? EstatEstructura { get; set; }

    [Range(1, 5)]
    [Column("estat_cobertes")]
    public int? EstatCobertes { get; set; }

    [Range(1, 5)]
    [Column("estat_facades")]
    public int? EstatFacanes { get; set; }

    [Range(1, 5)]
    [Column("estat_interiors")]
    public int? EstatInteriors { get; set; }

    // Observacions
    [Column("observacions")]
    public string? Observacions { get; set; }

    [Column("recomanacions")]
    public string? Recomanacions { get; set; }

    [NotMapped]
    public string? AccionsImmediates { get; set; }

    [Column("necessita_intervencio")]
    public bool SeguimentNecessari { get; set; } = false;

    [NotMapped]
    public DateTime? DataProperSeguiment { get; set; }

    // Inspector
    [MaxLength(200)]
    [Column("inspector")]
    public string InspectorNom { get; set; } = string.Empty;

    [NotMapped]
    public string? InspectorQualificacio { get; set; }

    [NotMapped]
    public string? InspectorEntitat { get; set; }

    // Control
    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [ForeignKey("PatrimoniId")]
    public virtual Patrimoni Patrimoni { get; set; } = null!;

    public virtual ICollection<Intervencio> Intervencions { get; set; } = new List<Intervencio>();

    // Propietats calculades
    [NotMapped]
    public string EstatColor => EstatGeneral switch
    {
        5 => "#28A745", // Excel·lent
        4 => "#6C757D", // Bo
        3 => "#FFC107", // Regular
        2 => "#FD7E14", // Dolent
        1 => "#DC3545", // Molt dolent
        _ => "#6C757D"
    };

    [NotMapped]
    public string EstatText => EstatGeneral switch
    {
        5 => "Excel·lent",
        4 => "Bo",
        3 => "Regular",
        2 => "Dolent",
        1 => "Molt dolent",
        _ => "Desconegut"
    };
}

/// <summary>
/// Intervencions al patrimoni
/// </summary>
[Table("PATR_intervencions")]
public class Intervencio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("patrimoni_id")]
    public int PatrimoniId { get; set; }

    [Column("inspeccio_id")]
    public int? InspeccioId { get; set; }

    // Informació bàsica
    [Column("tipus_intervencio")]
    public TipusIntervencio TipusIntervencio { get; set; }

    [NotMapped]
    public string Titol { get; set; } = string.Empty;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    // Planificació
    [Column("data_real_inici")]
    public DateTime? DataInici { get; set; }

    [Column("data_real_fi")]
    public DateTime? DataFi { get; set; }

    [Column("data_planificada_inici")]
    public DateTime? DataPrevistaInici { get; set; }

    [Column("data_planificada_fi")]
    public DateTime? DataPrevistaFi { get; set; }

    [Column("estat_intervencio")]
    public EstatIntervencio EstatIntervencio { get; set; } = EstatIntervencio.Planificada;

    // Pressupost
    [NotMapped]
    public decimal? PressupostEstimat { get; set; }

    [Column("pressupost_aprovat")]
    public decimal? PressupostAprovat { get; set; }

    [Column("cost_real")]
    public decimal? CostReal { get; set; }

    // Empreses i professionals
    [MaxLength(200)]
    [Column("empresa_contractista")]
    public string? EmpresaAdjudicataria { get; set; }

    [NotMapped]
    public string? DirectorObra { get; set; }

    [NotMapped]
    public string? ArquitecteResponsable { get; set; }

    // Resultats
    [NotMapped]
    public string? ObjectiusAssolits { get; set; }

    [NotMapped]
    public int? ValoracioResultats { get; set; }

    // Control
    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [ForeignKey("PatrimoniId")]
    public virtual Patrimoni Patrimoni { get; set; } = null!;

    [ForeignKey("InspeccioId")]
    public virtual Inspeccio? Inspeccio { get; set; }

    // Propietats calculades
    [NotMapped]
    public string EstatColor => EstatIntervencio switch
    {
        EstatIntervencio.Planificada => "#6C757D",
        EstatIntervencio.EnCurs => "#007BFF",
        EstatIntervencio.Aturada => "#FFC107",
        EstatIntervencio.Finalitzada => "#28A745",
        EstatIntervencio.Cancellada => "#DC3545",
        _ => "#6C757D"
    };

    [NotMapped]
    public decimal? DesviamentPressupost => 
        PressupostAprovat.HasValue && CostReal.HasValue 
            ? CostReal - PressupostAprovat 
            : null;

    [NotMapped]
    public int? DuradaDies => 
        DataInici.HasValue && DataFi.HasValue 
            ? (int)(DataFi.Value - DataInici.Value).TotalDays 
            : null;
}

/// <summary>
/// Tipus d'inspecció
/// </summary>
public enum TipusInspeccio
{
    Rutinària,
    Extraordinària,
    Urgent,
    Seguiment
}

/// <summary>
/// Tipus d'intervenció
/// </summary>
public enum TipusIntervencio
{
    Manteniment,
    Restauració,
    Conservació,
    Rehabilitació,
    Consolidació
}

/// <summary>
/// Estat de la intervenció
/// </summary>
public enum EstatIntervencio
{
    Planificada,
    EnCurs,
    Aturada,
    Finalitzada,
    Cancellada
}
