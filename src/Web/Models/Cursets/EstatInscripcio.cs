namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Estat d'una inscripció (<see cref="AlumneCurset"/>) a un curset.
    /// L'adjudicació de places NO és per ordre d'arribada: qui s'apunta durant el
    /// període entra en un sorteig; els no admesos van a la llista d'espera.
    /// </summary>
    public enum EstatInscripcio
    {
        /// <summary>Presentada des de la web, pendent del sorteig.</summary>
        Sollicitada = 0,

        /// <summary>Té plaça. És l'únic estat que es factura a les liquidacions.</summary>
        Admesa = 1,

        /// <summary>El sorteig no li ha adjudicat plaça; espera una baixa (vegeu <see cref="AlumneCurset.OrdreLlistaEspera"/>).</summary>
        LlistaEspera = 2,

        /// <summary>Tenia plaça i ha deixat el curset (<see cref="AlumneCurset.DataBaixa"/>).</summary>
        Baixa = 3,

        /// <summary>Sol·licitud descartada pel personal (ni admesa ni a la llista d'espera).</summary>
        Rebutjada = 4
    }
}
