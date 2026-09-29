namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Estat d'una liquidació (autoliquidació / carta de pagament). Es guarda com a
    /// text a `curs_liquidacions.Estat` per llegibilitat directa a la BD.
    /// </summary>
    public enum EstatLiquidacio
    {
        /// <summary>Emesa i pendent de cobrament.</summary>
        Emesa = 1,

        /// <summary>Cobrada (el cobrament real es gestiona fora de l'app; aquí només se'n registra la data).</summary>
        Cobrada = 2,

        /// <summary>Anul·lada (no s'ha de cobrar). Es conserva per a l'auditoria.</summary>
        Anullada = 3
    }
}
