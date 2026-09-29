using System.Collections.Generic;

namespace AppAjuntament.Models.Electoral
{
    public class ApiElectoralResponse
    {
        public bool success { get; set; }
        public ApiElectoralResult? result { get; set; }
        public List<ElectoralRecord>? Results { get; set; }
        public int Total { get; set; }
    }
}
