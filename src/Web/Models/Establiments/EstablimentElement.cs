namespace AppAjuntament.Models.Establiments
{
    public class EstablimentElement
    {
        public string? Id { get; set; }
        public string? Nom { get; set; }
        public string? Tipus { get; set; }
        public EstablimentAdreca? adreca { get; set; }
        public string? Municipi { get; set; }
        public string? nom_comercial { get; set; }
        public string? rao_social_nom { get; set; }
        public string? rao_social_nif { get; set; }
        public string? sector_economic { get; set; }
        public string? descripcio_activitat { get; set; }
        public string? altres_activitats { get; set; }
        public string? descripcio_nace { get; set; }
        public string? codi_nace { get; set; }
        public string? data_llic { get; set; }
        public string? annex { get; set; }
    }
}
