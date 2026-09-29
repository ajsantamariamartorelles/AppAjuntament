namespace AppAjuntament.Models.Conveni
{
    public class Conveni
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Descripcio { get; set; }
        public DateTime? DataInici { get; set; }
        public DateTime? DataFi { get; set; }

        // Properties used by API and views
        public int? ANY_SIGNATURA { get; set; }
        public string? TITOL_CONVENI { get; set; }
        public string? MATERIA { get; set; }
        public bool IsLocal { get; set; }
        public DateTime? DATA_SIGNATURA { get; set; }
        public string? VIGENT { get; set; }
        public decimal? TOTAL_APORTACIONS_PREVISTES { get; set; }
        public string? PDF_CONVENI { get; set; }
        public long? CODI_ENS { get; set; }
    }
}
