namespace AppAjuntament.Models.Voluntariat
{
    public class EstatVoluntari
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public string? Color { get; set; }
        public bool Actiu { get; set; } = true;
        public int Ordre { get; set; } = 0;
        public virtual ICollection<Voluntari> Voluntaris { get; set; } = new List<Voluntari>();
    }
}
