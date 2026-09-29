namespace AppAjuntament.Models.Cursets
{
    /// <summary>Assistència d'un alumne a una sessió concreta.</summary>
    public class Assistencia
    {
        public int SessioId { get; set; }
        public virtual Sessio? Sessio { get; set; }

        public int AlumneId { get; set; }
        public virtual Alumne? Alumne { get; set; }

        public bool Present { get; set; } = true;
        public string? Nota { get; set; }
    }
}
