using System.Globalization;

namespace AjuntamentSantaMariaMartorelles.Models;

// Mirall dels DTOs de seguiment del backend (AppAjuntament:
// Models/Cursets/Dto/SeguimentDtos.cs). Només els fa servir el controlador (regidor).

public class ResumImportsDto
{
	public decimal Liquidat { get; set; }
	public decimal Cobrat { get; set; }
	public decimal Pendent { get; set; }
	public int NumLiquidacions { get; set; }
	public string? UltimaRemesaEtiqueta { get; set; }
	public DateTime? UltimaRemesaData { get; set; }

	public string LiquidatText => Format.Euros(Liquidat);
	public string CobratText => Format.Euros(Cobrat);
	public string PendentText => Format.Euros(Pendent);

	public string UltimaRemesaText => UltimaRemesaData is null
		? "Encara no s'ha emès cap liquidació"
		: $"Última liquidació: {UltimaRemesaEtiqueta} · emesa {UltimaRemesaData:dd/MM/yyyy}";
}

public class ResumSeguimentCursetDto
{
	public int CursetId { get; set; }
	public int NumAdmeses { get; set; }
	public int NumLlistaEspera { get; set; }
	public int NumPendents { get; set; }
	public decimal ImportPendent { get; set; }
	public bool TeLiquidacions { get; set; }
}

public class PersonaInscritaDto
{
	public int AlumneId { get; set; }
	public string NomComplet { get; set; } = string.Empty;
	public bool Empadronat { get; set; }
	/// <summary>"Admesa", "LlistaEspera", "Baixa" o "Sollicitada".</summary>
	public string Estat { get; set; } = string.Empty;
	public int? OrdreLlistaEspera { get; set; }
	public DateTime? DataSollicitud { get; set; }
	public string? Origen { get; set; }
	public DateTime DataAlta { get; set; }
	public DateTime? DataBaixa { get; set; }
	/// <summary>"Pendent", "AlCorrent", "SenseLiquidar" o "NoFactura".</summary>
	public string Situacio { get; set; } = string.Empty;
	public decimal ImportPendent { get; set; }
	public int NumPendents { get; set; }
	public DateTime? PendentMesAntic { get; set; }

	/// <summary>Només client: curset de la fila a la pantalla de cobraments (per obrir la fitxa).</summary>
	[System.Text.Json.Serialization.JsonIgnore]
	public int CursetId { get; set; }

	public string ImportPendentText => Format.Euros(ImportPendent);

	public bool EsAdmesa => Estat == "Admesa";
	public bool EsBaixa => Estat == "Baixa";
	public bool EsEspera => Estat == "LlistaEspera";
	public bool EsSollicitada => Estat == "Sollicitada";
	public bool TePendent => Situacio == "Pendent";

	public string TarifaText => Empadronat ? "Tarifa empadronats" : "Tarifa no empadronats";

	/// <summary>Línia secundària de la fila, segons l'estat de la inscripció.</summary>
	public string DetallText => Estat switch
	{
		"Baixa" => $"Baixa el {DataBaixa:dd/MM/yyyy} · alta el {DataAlta:dd/MM/yyyy}",
		"LlistaEspera" => $"Sol·licitud del {DataSollicitud ?? DataAlta:dd/MM/yyyy}",
		"Sollicitada" => $"Sol·licitud {OrigenText} del {DataSollicitud ?? DataAlta:dd/MM/yyyy}",
		_ => $"{TarifaText} · des del {DataAlta:dd/MM/yyyy}"
	};

	public string DetallPendentText => NumPendents == 1
		? $"1 liquidació pendent · {PendentMesAntic:dd/MM/yyyy}"
		: $"{NumPendents} liquidacions pendents · la més antiga del {PendentMesAntic:dd/MM/yyyy}";

	private string OrigenText => string.Equals(Origen, "Web", StringComparison.OrdinalIgnoreCase) ? "web" : "presencial";

	public string EstatText => Estat switch
	{
		"Admesa" => "Admesa",
		"LlistaEspera" => OrdreLlistaEspera is int n ? $"Llista d'espera · núm. {n}" : "Llista d'espera",
		"Baixa" => "Baixa",
		"Sollicitada" => "Pendent de sorteig",
		_ => Estat
	};

	/// <summary>Etiqueta de la dreta: situació de pagament o, si no factura, lloc a la llista d'espera.</summary>
	public Pill Etiqueta => Situacio switch
	{
		"Pendent" => Pill.Perill($"Pendent {Format.Euros(ImportPendent)}"),
		"AlCorrent" => Pill.Ok("Al corrent"),
		"NoFactura" when EsEspera => Pill.Avis(OrdreLlistaEspera is int n ? $"Espera núm. {n}" : "En espera"),
		"NoFactura" => Pill.Avis("Pendent de sorteig"),
		_ => Pill.Neutre("Sense liquidar")
	};

	public Pill EtiquetaEstat => Estat switch
	{
		"Admesa" => Pill.Ok(EstatText),
		"Baixa" => Pill.Neutre(EstatText),
		_ => Pill.Avis(EstatText)
	};
}

public class InscritesCursetDto
{
	public int CursetId { get; set; }
	public string Titol { get; set; } = string.Empty;
	public int? MaxPlaces { get; set; }
	public bool InscripcioOberta { get; set; }
	public ResumImportsDto Resum { get; set; } = new();
	public List<PersonaInscritaDto> Persones { get; set; } = new();
}

public class LiquidacioSeguimentDto
{
	public int Id { get; set; }
	public string Numero { get; set; } = string.Empty;
	public string PeriodeEtiqueta { get; set; } = string.Empty;
	public int NumSessions { get; set; }
	public decimal PreuPerSessio { get; set; }
	public decimal Import { get; set; }
	/// <summary>"Emesa", "Cobrada" o "Anullada".</summary>
	public string Estat { get; set; } = string.Empty;
	public DateTime DataEmissio { get; set; }
	public DateTime? DataCobrament { get; set; }
	public DateTime? DataAnullacio { get; set; }
	public string? MotiuAnullacio { get; set; }

	public bool EsAnullada => Estat == "Anullada";
	public string ImportText => Format.Euros(Import);

	public string DetallText => Estat switch
	{
		"Cobrada" => $"{NumSessions} sessions × {Format.Euros(PreuPerSessio)} · cobrada el {DataCobrament ?? DataEmissio:dd/MM/yyyy}",
		"Anullada" => string.IsNullOrWhiteSpace(MotiuAnullacio)
			? $"Anul·lada el {DataAnullacio ?? DataEmissio:dd/MM/yyyy}"
			: $"Anul·lada el {DataAnullacio ?? DataEmissio:dd/MM/yyyy} · {MotiuAnullacio}",
		_ => $"{NumSessions} sessions × {Format.Euros(PreuPerSessio)} · emesa el {DataEmissio:dd/MM/yyyy}"
	};

	public Pill Etiqueta => Estat switch
	{
		"Cobrada" => Pill.Ok("Cobrada"),
		"Anullada" => Pill.Neutre("Anul·lada"),
		_ => Pill.Perill("Pendent")
	};

	/// <summary>Les anul·lades es mostren atenuades.</summary>
	public double Opacitat => EsAnullada ? 0.6 : 1.0;
	public TextDecorations DecoracioImport => EsAnullada ? TextDecorations.Strikethrough : TextDecorations.None;
}

public class PersonaFitxaDto
{
	public PersonaInscritaDto Persona { get; set; } = new();
	public int CursetId { get; set; }
	public string CursetTitol { get; set; } = string.Empty;
	public decimal PreuPerSessio { get; set; }
	public int SessionsImpartides { get; set; }
	public int SessionsAssistides { get; set; }
	public DateTime? UltimaAssistencia { get; set; }
	public List<LiquidacioSeguimentDto> Liquidacions { get; set; } = new();
}

public class CobramentsCursetDto
{
	public int CursetId { get; set; }
	public string Titol { get; set; } = string.Empty;
	public ResumImportsDto Resum { get; set; } = new();
	public List<PersonaInscritaDto> Pendents { get; set; } = new();
}

public class CobramentsDto
{
	public ResumImportsDto Total { get; set; } = new();
	public List<CobramentsCursetDto> Cursets { get; set; } = new();
}

/// <summary>Etiqueta de color (text + fons) per als estats. Colors de Resources/Styles/Colors.xaml.</summary>
public record Pill(string Text, Color TextColor, Color Fons)
{
	public static Pill Ok(string text) => new(text, Recurs("Success"), Recurs("PrimaryLight"));
	public static Pill Perill(string text) => new(text, Recurs("Danger"), Recurs("DangerLight"));
	public static Pill Avis(string text) => new(text, Recurs("Warning"), Recurs("WarningLight"));
	public static Pill Neutre(string text) => new(text, Recurs("Neutral"), Recurs("NeutralLight"));

	private static Color Recurs(string clau)
		=> Application.Current?.Resources.TryGetValue(clau, out var valor) == true && valor is Color c
			? c
			: Colors.Gray;
}

public static class Format
{
	private static readonly CultureInfo Ca = new("ca-ES");

	public static string Euros(decimal import) => import.ToString("N2", Ca) + " €";
}
