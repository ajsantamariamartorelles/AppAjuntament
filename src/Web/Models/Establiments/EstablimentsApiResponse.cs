using System.Collections.Generic;

namespace AppAjuntament.Models.Establiments
{
    public class EstablimentsApiResponse
    {
        public List<EstablimentElement>? data { get; set; }
        public List<EstablimentElement>? elements { get; set; }
        public object? links { get; set; }
        public object? meta { get; set; }
    }
}
