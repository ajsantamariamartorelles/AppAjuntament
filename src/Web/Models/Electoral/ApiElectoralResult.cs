namespace AppAjuntament.Models.Electoral
{
    public class ApiElectoralResult
    {
        public int Id { get; set; }
        public string? Municipi { get; set; }
        public string? Partit { get; set; }
        public int Vots { get; set; }
        public List<ElectoralRecord>? records { get; set; }
    }
}
