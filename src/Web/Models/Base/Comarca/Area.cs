namespace AppAjuntament.Models.Base.Comarca
{
    public class Area
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public int? EnsId { get; set; }
        public virtual AppAjuntament.Models.Base.Ens.Ens? Ens { get; set; }
    }
}
