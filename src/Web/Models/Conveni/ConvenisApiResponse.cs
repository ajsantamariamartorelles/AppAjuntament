namespace AppAjuntament.Models.Conveni
{
    public class ConvenisApiResponse
    {
        public bool success { get; set; }
        public ConvenisApiResult? result { get; set; }
    }

    public class ConvenisApiResult
    {
        public List<Conveni>? records { get; set; }
        public int total { get; set; }
    }
}
