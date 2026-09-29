namespace AppAjuntament.Models.FontsNaturals;

public class FontsNaturalsResponse
{
    public List<FontNaturalFeature> features { get; set; } = new();
}

public class FontNaturalFeature
{
    public FontNaturalAttributes attributes { get; set; } = new();
}

public class FontNaturalAttributes
{
    public string? FDB_CODI_FONT { get; set; }
    public string? FDB_NOM_FONT { get; set; }
    public string? FDB_MUN_SOL { get; set; }
    public string? FDB_CODI_INE { get; set; }
    public string? FDB_COMARCA { get; set; }
    public int? FDB_ALTITUD { get; set; }
    public long? FDA_DATA_RECOLLIDA { get; set; }
    public double? FDA_E_COLI { get; set; }
    public double? FDA_ENTEROCOC { get; set; }
    public double? FDA_CLOSTRIDIUM { get; set; }
    public double? FDA_BACT_COLIFORMS { get; set; }
    public double? FDA_TERBOLESA { get; set; }
    public double? FDA_PH { get; set; }
    public double? FDA_CONDUCTIVITAT { get; set; }
    public double? FDA_TEMPERATURA { get; set; }
    public double? FDA_CABAL { get; set; }
    public string? FDB_FOTO { get; set; }
    public string? INFORME { get; set; }
    public int? CATEGORIA { get; set; }
    public string? FDA_TIP_ANALITICA { get; set; }
    public double? FDA_NITRAT { get; set; }

    public DateTime? DataRecollida => FDA_DATA_RECOLLIDA.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(FDA_DATA_RECOLLIDA.Value).LocalDateTime
        : null;
}
