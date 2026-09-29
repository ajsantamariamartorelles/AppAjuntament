namespace AppAjuntament.Models.IngressosExterns;

public class IngressosExternsDashboard
{
    public int Exercici { get; set; }
    public FonsCooperacioMunicipi? Municipi { get; set; }
    public decimal? MedianaMunicipisSimilarsPerHabitant { get; set; }
    public decimal? MedianaComarcaPerHabitant { get; set; }
    public int MunicipisComparables { get; set; }
    public decimal? PosicioPercentil { get; set; }
    public List<FonsCooperacioMunicipi> MunicipisSimilars { get; set; } = new();
    public List<FonsCooperacioHistoric> Historic { get; set; } = new();
    public List<PagamentPendent> PagamentsPendents { get; set; } = new();
}

public class PagamentPendent
{
    public string TipusAjut { get; set; } = string.Empty;
    public int Exercici { get; set; }
    public decimal Import { get; set; }
    public bool Retingut { get; set; }
    public DateTime? DataPublicacio { get; set; }
}

public class FonsCooperacioMunicipi
{
    public string CodiIne6 { get; set; } = string.Empty;
    public string Municipi { get; set; } = string.Empty;
    public string Comarca { get; set; } = string.Empty;
    public int Exercici { get; set; }
    public decimal Import { get; set; }
    public decimal? ImportComplementari { get; set; }
    public decimal Total => Import + (ImportComplementari ?? 0);
    public int Poblacio { get; set; }
    public decimal ImportPerHabitant => Poblacio > 0 ? Total / Poblacio : 0;
    public bool EsMunicipiActual { get; set; }
}

public class FonsCooperacioHistoric
{
    public int Exercici { get; set; }
    public decimal Import { get; set; }
    public decimal? ImportComplementari { get; set; }
    public decimal Total => Import + (ImportComplementari ?? 0);
}
