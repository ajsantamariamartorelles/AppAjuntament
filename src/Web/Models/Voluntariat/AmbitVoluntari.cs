namespace AppAjuntament.Models.Voluntariat
{
    public class AmbitVoluntari
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public bool Actiu { get; set; } = true;
        public bool PotSerCoordinador { get; set; } = false;
        public bool PotSerEnllacAjuntament { get; set; } = false;
        public DateTime DataCreacio { get; set; } = DateTime.Now;
        public virtual ICollection<SubambitVoluntari> Subambits { get; set; } = new List<SubambitVoluntari>();
        public virtual ICollection<VoluntariAmbit> VoluntarisAmbits { get; set; } = new List<VoluntariAmbit>();
    }
}
