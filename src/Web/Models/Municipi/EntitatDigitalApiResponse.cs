namespace AppAjuntament.Models.Municipi
{
    public class EntitatDigitalApiResponse
    {
        public bool success { get; set; }
        public EntitatDigitalApiResult? result { get; set; }
    }

    public class EntitatDigitalApiResult
    {
        public List<EntitatDigitalRecord>? records { get; set; }
        public int total { get; set; }
    }
}
