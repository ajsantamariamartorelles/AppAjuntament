using System.Collections.Generic;

namespace AppAjuntament.Models.Base.Comarca
{
    public class ApiDataset
    {
        public string? nom { get; set; }
        public string? machinename { get; set; }
        public string? descripcio { get; set; }
        public List<string>? paraules_clau { get; set; }
        public string? llicencia { get; set; }
        public int freq_actualitzacio { get; set; }
        public List<string>? sector { get; set; }
        public List<string>? tema { get; set; }
        public string? responsable { get; set; }
        public string? idioma { get; set; }
        public string? home_page { get; set; }
        public List<ApiReferencia>? referencies { get; set; }
        public string? tipus { get; set; }
        public string? estat { get; set; }
        public string? creacio { get; set; }
        public string? modificacio { get; set; }
        public int entitats { get; set; }
        public List<ApiComarcaElement>? elements { get; set; }
    }
}
