namespace AppAjuntament.Models.Voluntariat
{
    public class Formacio
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public string? Formador { get; set; }
        public string? Ubicacio { get; set; }
        public DateTime DataInici { get; set; } = DateTime.Now;
        public DateTime? DataFi { get; set; }
        public DateTime DataCreacio { get; set; } = DateTime.Now;
        public bool Activa { get; set; } = true;
        public int? DuradaHores { get; set; }

        // FK & Navigation
        public int? EnsId { get; set; }
        public virtual AppAjuntament.Models.Base.Ens.Ens? Ens { get; set; }

        public virtual ICollection<FormacioVoluntari> Voluntaris { get; set; } = new List<FormacioVoluntari>();
    }
}
