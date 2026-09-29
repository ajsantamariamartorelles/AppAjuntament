using AppAjuntament.Models.Base.Sexe;
using AppAjuntament.Models.Base.Usuari;

namespace AppAjuntament.Models.Tercers;

/// <summary>
/// Taula genèrica de tercers (persones físiques).
/// Actua com a capa base reutilitzable per Voluntaris, Titulars d'Armes, etc.
/// </summary>
public class Tercer
{
    public int Id { get; set; }

    // Identificació
    public string Nom { get; set; } = string.Empty;
    public string? Cognoms { get; set; }
    public string? DNI { get; set; }
    public DateTime? DataNaixement { get; set; }

    // FK Sexe
    public int? SexeId { get; set; }
    public virtual Sexe? Sexe { get; set; }

    // Contacte
    public string? Email { get; set; }
    public string? Telefon { get; set; }

    // Domicili
    public string? Adreca { get; set; }
    public string? CodiPostal { get; set; }
    public string? Poblacio { get; set; }

    // FKs geogràfiques (tipus han de coincidir amb els PKs reals)
    public string? MunicipiIne { get; set; }   // → municipis(ine) VARCHAR(10)
    public string? ProvinciaId { get; set; }   // → provincies(Id) VARCHAR(10)
    public int? ComarcaCodi { get; set; }       // → comarques(Codi) INT

    // Auditoria
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public virtual Usuari? CreatedByUser { get; set; }
    public int? UpdatedBy { get; set; }
    public virtual Usuari? UpdatedByUser { get; set; }

    /// <summary>La persona ha signat l'autorització de tractament de dades per als
    /// serveis de l'Ajuntament. Es confirma en donar d'alta el tercer (AGENTS.md);
    /// en editar-lo ja no es torna a demanar.</summary>
    public bool AutoritzacioTractamentSignada { get; set; }
    public DateTime? DataAutoritzacio { get; set; }

    // Computed
    public string NomComplet => string.IsNullOrWhiteSpace(Cognoms)
        ? Nom
        : $"{Nom} {Cognoms}".Trim();

    public int? Edat => DataNaixement.HasValue
        ? (int)((DateTime.Now - DataNaixement.Value).TotalDays / 365.25)
        : null;

    // Relació inversa (opcional, lazy)
    public virtual ICollection<Voluntariat.Voluntari> Voluntaris { get; set; } = new List<Voluntariat.Voluntari>();
}
