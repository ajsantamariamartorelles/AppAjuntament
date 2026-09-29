namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Dades específiques de Cursets per a un alumne.
    /// La identitat i el contacte (nom, cognoms, telèfon, email...) viuen a
    /// AppAjuntament.Models.Tercers.Tercer, la capa base reutilitzable del repo
    /// (mateix patró que fa servir Voluntariat.Voluntari).
    /// </summary>
    public class Alumne
    {
        public int Id { get; set; }

        // FK → Tercers (identitat i contacte)
        public int TercerId { get; set; }
        public virtual AppAjuntament.Models.Tercers.Tercer? Tercer { get; set; }

        public string? Notes { get; set; }
        public bool Actiu { get; set; } = true;

        /// <summary>Empadronat/a al municipi. Determina quin preu per sessió se li aplica als cursets amb cost.</summary>
        public bool Empadronat { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Computed (delega a Tercer)
        public string NomComplet => Tercer?.NomComplet ?? "(sense persona)";

        // Collections
        public virtual ICollection<AlumneCurset> Cursets { get; set; } = new List<AlumneCurset>();
        public virtual ICollection<Assistencia> Assistencies { get; set; } = new List<Assistencia>();
    }
}
