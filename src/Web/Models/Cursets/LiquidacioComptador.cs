namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Comptador correlatiu de liquidacions. La sèrie del número és global dins un
    /// període: la clau és "{any}-{CodiPeriode}" (p. ex. "2026-1T") i s'incrementa en
    /// emetre cada liquidació, sigui quin sigui el curset o el servei.
    /// </summary>
    public class LiquidacioComptador
    {
        /// <summary>"{any}-{CodiPeriode}", p. ex. "2026-1T".</summary>
        public string Clau { get; set; } = string.Empty;

        /// <summary>Últim número de sèrie assignat.</summary>
        public int Ultim { get; set; }
    }
}
