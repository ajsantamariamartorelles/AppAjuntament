namespace AppAjuntament.Models.Voluntariat
{
    public class TipologiaVoluntari
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public bool Actiu { get; set; } = true;
    }
}
