namespace AppAjuntament.Models.Voluntariat
{
    public class Activitat
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public string? Ubicacio { get; set; }
        public DateTime DataInici { get; set; } = DateTime.Now;
        public DateTime? DataFi { get; set; }
        public DateTime DataCreacio { get; set; } = DateTime.Now;
        public bool Activa { get; set; } = true;

        // FK & Navigation
        public int? EnsId { get; set; }
        public virtual AppAjuntament.Models.Base.Ens.Ens? Ens { get; set; }
        public int? AreaId { get; set; }
        public virtual AppAjuntament.Models.Base.Comarca.Area? Area { get; set; }

        public virtual ICollection<ActitatVoluntari> Voluntaris { get; set; } = new List<ActitatVoluntari>();
    }
}
