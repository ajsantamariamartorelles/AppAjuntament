namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Configuració de la plantilla d'autoliquidació (una sola fila, Id = 1).
    /// Els textos són editables des de la web sense necessitat de publicar.
    ///
    /// Les propietats de text són anul·lables perquè les columnes de la BD ho són
    /// (una fila creada per codi sempre les té amb valor, però un camp buidat a mà
    /// pot quedar NULL). El codi que les consumeix ha de tolerar null.
    ///
    /// Tokens admesos als patrons: {ANY}, {PERIODE}, {PERIODE_ETIQUETA},
    /// {TIPUS}, {CURSET}.
    /// </summary>
    public class LiquidacioConfig
    {
        public int Id { get; set; } = 1;

        /// <summary>Patró del número d'expedient de la remesa. P. ex. "CUR/{ANY}/LIQ-{PERIODE}".</summary>
        public string? ExpedientPatro { get; set; } = "CUR/{ANY}/LIQ-{PERIODE}";

        /// <summary>Text de "Ordenança i/o tarifa aplicable" per defecte (secció 3).</summary>
        public string? OrdenancaTarifa { get; set; } = string.Empty;

        /// <summary>Patró del text de "Concepte / activitat" (secció 3). P. ex. "{TIPUS} - {CURSET} ({PERIODE_ETIQUETA})".</summary>
        public string? ConceptePatro { get; set; } = "{TIPUS} - {CURSET} ({PERIODE_ETIQUETA})";

        /// <summary>Text de "Entitats col·laboradores / comptes / mitjans de pagament" (secció 6).</summary>
        public string? EntitatsColaboradoresText { get; set; } = string.Empty;

        /// <summary>Text de "Pagament a les Oficines municipals" (secció 6).</summary>
        public string? MitjansPagamentText { get; set; } = string.Empty;

        /// <summary>Text de "Oficina cobratòria" (secció 10).</summary>
        public string? OficinaCobratoriaText { get; set; } = string.Empty;

        /// <summary>Text d'"Instruccions de recursos" (secció 7).</summary>
        public string? TextRecursos { get; set; } = string.Empty;

        /// <summary>Text d'"Important" (secció 8).</summary>
        public string? TextImportant { get; set; } = string.Empty;

        /// <summary>Text de "Terminis cobratoris" (secció 9).</summary>
        public string? TextTerminis { get; set; } = string.Empty;

        /// <summary>Nom que apareix a la capçalera del document.</summary>
        public string? CapcaleraMunicipi { get; set; } = "Ajuntament de Santa Maria de Martorelles";

        public DateTime? UpdatedAt { get; set; }
    }
}
