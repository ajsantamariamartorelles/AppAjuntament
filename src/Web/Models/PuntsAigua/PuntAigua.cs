using System.Text.Json.Serialization;

namespace AppAjuntament.Models.PuntsAigua;

public class PuntAigua
{
    [JsonPropertyName("Municipi")]
    public string? Municipi { get; set; }

    [JsonPropertyName("Codi")]
    public string? Codi { get; set; }

    [JsonPropertyName("Codi_unic")]
    public string? CodiUnic { get; set; }

    [JsonPropertyName("Categoria")]
    public string? Categoria { get; set; }

    [JsonPropertyName("Tipologia")]
    public string? Tipologia { get; set; }

    [JsonPropertyName("Tipus")]
    public string? Tipus { get; set; }

    [JsonPropertyName("Paratge")]
    public string? Paratge { get; set; }

    [JsonPropertyName("Estat")]
    public string? Estat { get; set; }

    [JsonPropertyName("Foto1")]
    public string? Foto1 { get; set; }

    [JsonPropertyName("Foto2")]
    public string? Foto2 { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("long")]
    public double? Long { get; set; }

    public bool TeFoto1 => !string.IsNullOrWhiteSpace(Foto1);
    public bool TeFoto2 => !string.IsNullOrWhiteSpace(Foto2) && Foto2.Trim() != " ";
}
