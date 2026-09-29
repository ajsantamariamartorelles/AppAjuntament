namespace AppAjuntament.Models.Voluntariat
{
    public class FormacioVoluntari
    {
        public int FormacioId { get; set; }
        public int VoluntariId { get; set; }
        public string? Observacions { get; set; }
        public DateTime DataInscripcio { get; set; } = DateTime.Now;
        public bool Actiu { get; set; } = true;
        public bool Superat { get; set; } = false;
        public DateTime? DataCompletat { get; set; }

        public virtual Formacio? Formacio { get; set; }
        public virtual Voluntari? Voluntari { get; set; }
    }
}
