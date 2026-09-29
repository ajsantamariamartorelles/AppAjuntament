namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Configuració del correu de baixa per absències consecutives: connexió SMTP i
    /// plantilla del missatge. Una sola fila (Id = 1), editable des de la web
    /// (<c>/cursets/absencies/configuracio</c>) sense necessitat de publicar.
    ///
    /// Tokens admesos a <see cref="Assumpte"/> i <see cref="CosHtml"/>: {NOM}, {CURSET},
    /// {INSTITUCIO}, {MISSATGE}.
    /// </summary>
    public class EmailAbsenciesConfig
    {
        public const string CosHtmlPerDefecte =
            "<p>Hola {NOM},</p>" +
            "<p>T'informem que s'ha tramitat la teva baixa del curset <strong>{CURSET}</strong> " +
            "per no haver-hi assistit durant diverses sessions consecutives sense justificar.</p>" +
            "{MISSATGE}" +
            "<p>Si creus que es tracta d'un error o vols justificar les absències, posa't en contacte amb {INSTITUCIO}.</p>" +
            "<p>{INSTITUCIO}</p>";

        public int Id { get; set; } = 1;

        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public bool SmtpSsl { get; set; } = true;
        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }

        public string? RemitentEmail { get; set; }
        public string? RemitentNom { get; set; } = "Ajuntament de Santa Maria de Martorelles";

        /// <summary>Assumpte del correu. Tokens: {NOM}, {CURSET}, {INSTITUCIO}.</summary>
        public string? Assumpte { get; set; } = "Baixa del curset «{CURSET}» per inassistència";
        /// <summary>Cos HTML del correu. Tokens: {NOM}, {CURSET}, {INSTITUCIO}, {MISSATGE}.</summary>
        public string? CosHtml { get; set; } = CosHtmlPerDefecte;

        public DateTime? UpdatedAt { get; set; }
    }
}
