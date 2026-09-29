using System.Collections.Generic;

namespace AppAjuntament.Models.CIDO
{
    public class OrdenancesApiResponse
    {
        public List<OrdenancaData>? data { get; set; }
        public OrdenancesLinks? links { get; set; }
        public OrdenancesMeta? meta { get; set; }
    }
}
