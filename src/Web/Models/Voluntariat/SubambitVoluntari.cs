namespace AppAjuntament.Models.Voluntariat
{
    public class SubambitVoluntari
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public bool Actiu { get; set; } = true;
        public DateTime DataCreacio { get; set; } = DateTime.Now;
        public int AmbitVoluntariId { get; set; }
        public AmbitVoluntari? AmbitVoluntari { get; set; }
        public virtual ICollection<VoluntariAmbit> VoluntarisAmbits { get; set; } = new List<VoluntariAmbit>();
    }
}
