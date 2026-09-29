namespace AppAjuntament.Models.CIDO
{
    public class OrdenancaData
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public OrdenancaLinks? links { get; set; }
        public OrdenancaAttributes? attributes { get; set; }
        public OrdenancaRelationships? relationships { get; set; }
    }
}
