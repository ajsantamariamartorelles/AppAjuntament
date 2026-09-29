namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Una remesa d'emissió de liquidacions: el resultat d'executar el procés de
    /// liquidació per a un període (trimestre, mes, setmana o dates lliures).
    /// Agrupa totes les <see cref="Liquidacio"/> generades en una sola passada, per
    /// poder tornar a descarregar el ZIP i per auditoria.
    /// </summary>
    public class LiquidacioRemesa
    {
        public int Id { get; set; }

        /// <summary>Descripció lliure ("Pilates 1r trimestre 2026").</summary>
        public string Descripcio { get; set; } = string.Empty;

        /// <summary>"Trimestral", "Mensual", "Setmanal" o "Lliure".</summary>
        public string CadenciaTipus { get; set; } = "Trimestral";

        /// <summary>Codi del període usat al número ("1T", "GEN", "S05", "LLIURE").</summary>
        public string CodiPeriode { get; set; } = string.Empty;

        /// <summary>Etiqueta llegible del període ("1r trimestre 2026").</summary>
        public string PeriodeEtiqueta { get; set; } = string.Empty;

        public DateTime PeriodeInici { get; set; }
        public DateTime PeriodeFi { get; set; }

        /// <summary>Número d'expedient resolt del patró configurat.</summary>
        public string Expedient { get; set; } = string.Empty;

        public DateTime DataCreacio { get; set; } = DateTime.Now;

        /// <summary>Usuari municipal que ha emès la remesa (opcional).</summary>
        public int? CreatedByUserId { get; set; }

        public int NumLiquidacions { get; set; }
        public decimal ImportTotal { get; set; }

        public virtual ICollection<Liquidacio> Liquidacions { get; set; } = new List<Liquidacio>();
    }
}
