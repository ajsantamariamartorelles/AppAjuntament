namespace AppAjuntament.Models.Voluntariat
{
    public class Voluntari
    {
        public int Id { get; set; }

        // FK → Tercers (capa de dades personals genèrica)
        public int? TercerId { get; set; }
        public virtual AppAjuntament.Models.Tercers.Tercer? Tercer { get; set; }

        // Camp específic del voluntariat (no a Tercers)
        public string? TelefonEmergencia { get; set; }
        public string? MotiuBaixa { get; set; }
        public string? Habilitats { get; set; }
        public string? Disponibilitat { get; set; }
        public string? Observacions { get; set; }
        public string? RutaAcordSignat { get; set; }
        public DateTime DataIncorporacio { get; set; } = DateTime.Now;
        public bool Actiu { get; set; }

        // Computed properties
        public string NomComplet => Tercer?.NomComplet ?? "(sense persona)";
        public bool EsCoordinador => CoordinadorId == null && VoluntarisCoordinats?.Any() == true;
        public bool EsEnllacAjuntament => EnllacAjuntamentId == null && VoluntarisEnllacats?.Any() == true;

        // DateBaixa
        public DateTime? DataBaixa { get; set; }
        public bool AcordSignat { get; set; } = false;
        public DateTime? DataSignaturaAcord { get; set; }

        // Computed (delega a Tercer)
        public int? Edat => Tercer?.Edat;

        // FK & Navigation - EstatVoluntari
        public int? EstatVoluntariId { get; set; }
        public virtual EstatVoluntari? EstatVoluntari { get; set; }

        // FK & Navigation - Ens
        public int? EnsId { get; set; }
        public virtual AppAjuntament.Models.Base.Ens.Ens? Ens { get; set; }

        // Self-referencing - EnllacAjuntament
        public int? EnllacAjuntamentId { get; set; }
        public virtual Voluntari? EnllacAjuntament { get; set; }
        public virtual ICollection<Voluntari> VoluntarisEnllacats { get; set; } = new List<Voluntari>();

        // Self-referencing - Coordinador
        public int? CoordinadorId { get; set; }
        public virtual Voluntari? Coordinador { get; set; }
        public virtual ICollection<Voluntari> VoluntarisCoordinats { get; set; } = new List<Voluntari>();

        // Collections
        public virtual ICollection<VoluntariAmbit> VoluntarisAmbits { get; set; } = new List<VoluntariAmbit>();
        public virtual ICollection<ActitatVoluntari> Activitats { get; set; } = new List<ActitatVoluntari>();
        public virtual ICollection<FormacioVoluntari> Formacions { get; set; } = new List<FormacioVoluntari>();
    }
}
