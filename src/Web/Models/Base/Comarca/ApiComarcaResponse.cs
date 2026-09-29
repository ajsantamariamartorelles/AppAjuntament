using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppAjuntament.Models.Base.Comarca
{
    public class ApiComarcaResponse
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string nom { get; set; }
        public required string machinename { get; set; }
        public required string descripcio { get; set; }
        public int entitats { get; set; }
        public required List<ApiDataset> datasets { get; set; }
    }
}
