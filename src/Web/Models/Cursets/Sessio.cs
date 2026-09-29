namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Una classe concreta d'un curset en una data: es crea quan la professora
    /// "inicia classe" (Oberta) i es tanca quan desa l'assistència (Tancada).
    /// </summary>
    public class Sessio
    {
        public int Id { get; set; }

        public int CursetId { get; set; }
        public virtual Curset? Curset { get; set; }

        // Professora que ha impartit realment la sessió (normalment = Curset.ProfessoraId)
        public int ProfessoraId { get; set; }
        public virtual AppAjuntament.Models.Base.Usuari.Usuari? Professora { get; set; }

        public DateTime Data { get; set; } // Guardat com a DATE (Fluent config: HasColumnType("date"))
        public EstatSessio Estat { get; set; } = EstatSessio.Oberta;
        public string? NotaSessio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        public virtual ICollection<Assistencia> Assistencies { get; set; } = new List<Assistencia>();
    }
}
