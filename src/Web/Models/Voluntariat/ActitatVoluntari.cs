namespace AppAjuntament.Models.Voluntariat
{
    public class ActitatVoluntari
    {
        public int ActivitatId { get; set; }
        public int VoluntariId { get; set; }
        public string? Observacions { get; set; }
        public DateTime DataInscripcio { get; set; } = DateTime.Now;
        public bool Actiu { get; set; } = true;

        public virtual Activitat? Activitat { get; set; }
        public virtual Voluntari? Voluntari { get; set; }
    }
}
