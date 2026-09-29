namespace AppAjuntament.Models.Cursets.Dto
{
    // DTOs de seguiment per al controlador (regidor) a l'app MAUI: persones
    // inscrites a cada curset i situació de les seves liquidacions. Només lectura.

    /// <summary>
    /// Situació de pagament d'una persona en un curset (derivada de les seves liquidacions):
    /// "Pendent" (alguna Emesa), "AlCorrent" (totes cobrades/anul·lades),
    /// "SenseLiquidar" (cap liquidació vàlida) o "NoFactura" (no té plaça: espera/sol·licitud).
    /// </summary>
    public static class SituacioPagament
    {
        public const string Pendent = "Pendent";
        public const string AlCorrent = "AlCorrent";
        public const string SenseLiquidar = "SenseLiquidar";
        public const string NoFactura = "NoFactura";
    }

    /// <summary>Imports d'un conjunt de liquidacions (les anul·lades no hi compten).</summary>
    public class ResumImportsDto
    {
        public decimal Liquidat { get; set; }
        public decimal Cobrat { get; set; }
        public decimal Pendent { get; set; }
        public int NumLiquidacions { get; set; }

        /// <summary>Etiqueta del període de la darrera remesa emesa ("2n trimestre 2026").</summary>
        public string? UltimaRemesaEtiqueta { get; set; }
        public DateTime? UltimaRemesaData { get; set; }
    }

    /// <summary>Resum per a la targeta del curset a la llista del regidor.</summary>
    public class ResumSeguimentCursetDto
    {
        public int CursetId { get; set; }
        public int NumAdmeses { get; set; }
        public int NumLlistaEspera { get; set; }
        /// <summary>Persones (admeses o de baixa) amb alguna liquidació pendent.</summary>
        public int NumPendents { get; set; }
        public decimal ImportPendent { get; set; }
        public bool TeLiquidacions { get; set; }
    }

    /// <summary>Una persona a la pestanya "Inscrites" d'un curset.</summary>
    public class PersonaInscritaDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public bool Empadronat { get; set; }

        /// <summary>Nom de l'<see cref="EstatInscripcio"/> ("Admesa", "LlistaEspera", "Baixa", "Sollicitada").</summary>
        public string Estat { get; set; } = string.Empty;
        public int? OrdreLlistaEspera { get; set; }
        public DateTime? DataSollicitud { get; set; }
        public string? Origen { get; set; }
        public DateTime DataAlta { get; set; }
        public DateTime? DataBaixa { get; set; }

        /// <summary>Constant de <see cref="SituacioPagament"/>.</summary>
        public string Situacio { get; set; } = SituacioPagament.SenseLiquidar;
        public decimal ImportPendent { get; set; }
        public int NumPendents { get; set; }
        /// <summary>Data d'emissió de la liquidació pendent més antiga.</summary>
        public DateTime? PendentMesAntic { get; set; }
    }

    /// <summary>Pestanya "Inscrites" d'un curset.</summary>
    public class InscritesCursetDto
    {
        public int CursetId { get; set; }
        public string Titol { get; set; } = string.Empty;
        public int? MaxPlaces { get; set; }
        public bool InscripcioOberta { get; set; }
        public ResumImportsDto Resum { get; set; } = new();
        public List<PersonaInscritaDto> Persones { get; set; } = new();
    }

    /// <summary>Una liquidació tal com la veu el regidor.</summary>
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
    }

    /// <summary>Fitxa d'una persona dins d'un curset (sense DNI, adreça ni contacte).</summary>
    public class PersonaFitxaDto
    {
        public PersonaInscritaDto Persona { get; set; } = new();
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public decimal PreuPerSessio { get; set; }

        /// <summary>Sessions tancades del curset mentre hi estava apuntada.</summary>
        public int SessionsImpartides { get; set; }
        public int SessionsAssistides { get; set; }
        public DateTime? UltimaAssistencia { get; set; }

        public List<LiquidacioSeguimentDto> Liquidacions { get; set; } = new();
    }

    /// <summary>Un curset dins del resum global de cobraments.</summary>
    public class CobramentsCursetDto
    {
        public int CursetId { get; set; }
        public string Titol { get; set; } = string.Empty;
        public ResumImportsDto Resum { get; set; } = new();
        /// <summary>Només les persones amb alguna liquidació pendent, de més a menys import.</summary>
        public List<PersonaInscritaDto> Pendents { get; set; } = new();
    }

    public class CobramentsDto
    {
        public ResumImportsDto Total { get; set; } = new();
        public List<CobramentsCursetDto> Cursets { get; set; } = new();
    }
}
