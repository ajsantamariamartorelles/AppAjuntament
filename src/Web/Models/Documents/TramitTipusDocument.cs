using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Documents;

/// <summary>
/// Relació N:M entre tràmits i tipus de document.
/// Mapeig: Rel_TramitTipusDocument
/// </summary>
[Table("Rel_TramitTipusDocument")]
public class TramitTipusDocument
{
    public int Id { get; set; }

    public int TramitId { get; set; }

    public int TipusDocumentId { get; set; }

    public bool EsObligatori { get; set; }

    public int Ordre { get; set; } = 100;

    public bool Actiu { get; set; } = true;

    public virtual Tramit? Tramit { get; set; }

    public virtual TipusDocument? TipusDocument { get; set; }
}
