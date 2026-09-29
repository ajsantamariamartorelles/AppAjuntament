namespace AppAjuntament.Models.Base.Comarca
{
    public class ApiComarcaElement
    {
        public int id { get; set; }
        public string? nom { get; set; }
        public string? comarca_id { get; set; }
        public string? comarca_nom { get; set; }
        public string? comarca_geo { get; set; }
        public string? nom_dbpedia { get; set; }
        public ApiGrupCC? grup_cc { get; set; }
    }

    public class ApiGrupCC
    {
        public string? cc_adreca_completa { get; set; }
        public string? cc_adreca { get; set; }
        public string? cc_cpostal { get; set; }
        public List<string>? cc_email { get; set; }
        public string? cc_fax { get; set; }
        public string? cc_web { get; set; }
    }
}
