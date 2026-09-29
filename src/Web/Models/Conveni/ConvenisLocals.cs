namespace AppAjuntament.Models.Conveni
{
    public class ConvenisLocals : Conveni
    {
        public string? Municipi { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? _Id { get; set; }

        // Extra fields used in views
        public DateTime? DATA_VIGENCIA { get; set; }
        public string? PRORROGABLE { get; set; }
        public int? ProrroguesPermeses { get; set; }
        public string? OBJECTE { get; set; }
        public decimal? APORTACIONS_PREVISTES_ENS_LOCALS { get; set; }
        public string? Notes { get; set; }
        public string? ORGANISMES_SIGNANTS_ENS_LOCALS { get; set; }
    }
}
