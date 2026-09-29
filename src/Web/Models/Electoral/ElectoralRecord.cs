using System.Text.Json.Serialization;

namespace AppAjuntament.Models.Electoral
{
    public class ElectoralRecord
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }
        public string? MUNICIPI { get; set; }
        [JsonPropertyName("NOM")]
        public string? Partit { get; set; }
        public string? SIGLES { get; set; }
        public int VOTS { get; set; }
        [JsonPropertyName("VOTS_PERCENT")]
        public double Percentatge { get; set; }
        [JsonPropertyName("REGIDORS")]
        public int Escons { get; set; }
        public int ANY_ELECCIO { get; set; }
        public long? CODI_ENS { get; set; }

        // Aliases per compatibilitat amb la vista
        [JsonIgnore]
        public string? Municipi => MUNICIPI;
        [JsonIgnore]
        public string? Sigles => SIGLES;
    }
}
