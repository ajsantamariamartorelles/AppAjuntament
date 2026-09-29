namespace AppAjuntament.Models.Cursets
{
    /// <summary>Catàleg tancat de temàtiques de curset (Pilates, Ioga, Zumba...).</summary>
    public class TipusCurset
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public bool Actiu { get; set; } = true;

        /// <summary>Codi curt del servei per als números de liquidació (p. ex. "PTES" = Pilates).</summary>
        public string? CodiLiquidacio { get; set; }

        public virtual ICollection<Curset> Cursets { get; set; } = new List<Curset>();
    }
}
