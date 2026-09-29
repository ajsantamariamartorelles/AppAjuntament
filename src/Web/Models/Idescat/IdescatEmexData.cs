namespace AppAjuntament.Models.Idescat;

public class IdescatEmexData
{
    public string CodiMunicipi { get; set; } = string.Empty;

    public string NomMunicipi { get; set; } = string.Empty;

    public string? NomComarca { get; set; }

    public string? NomCatalunya { get; set; }

    public List<IdescatEmexIndicatorRow> Indicadors { get; set; } = new();
}

public class IdescatEmexIndicatorRow
{
    public string GrupId { get; set; } = string.Empty;

    public string GrupNom { get; set; } = string.Empty;

    public string TaulaId { get; set; } = string.Empty;

    public string TaulaNom { get; set; } = string.Empty;

    public string IndicadorId { get; set; } = string.Empty;

    public string IndicadorNom { get; set; } = string.Empty;

    public string? Unitat { get; set; }

    public string? ReferenciaTemporal { get; set; }

    public DateTimeOffset? DataActualitzacio { get; set; }

    public string ValorMunicipiText { get; set; } = "_";

    public string ValorComarcaText { get; set; } = "_";

    public string ValorCatalunyaText { get; set; } = "_";

    public decimal? ValorMunicipi { get; set; }

    public decimal? ValorComarca { get; set; }

    public decimal? ValorCatalunya { get; set; }

    public string? Font { get; set; }

    public string? Enllac { get; set; }
}
