namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Una autoliquidació / carta de pagament registrada per a una (alumna × curset)
    /// en un període. Es factura per dia de classe impartit (sessió Tancada) mentre
    /// l'alumna hi estava apuntada, al preu segons empadronament.
    ///
    /// Els camps *snapshot (nom, NIF, adreça, títol del curset, concepte...) es
    /// congelen en el moment d'emetre-la perquè una reimpressió sigui estable encara
    /// que després canviïn les dades de l'alumna o del curset.
    /// </summary>
    public class Liquidacio
    {
        public int Id { get; set; }

        /// <summary>Número oficial: {CodiServei}-{CodiPeriode}-{CodiCurset}-{Serie:000} (p. ex. "PTES-1T-DVT-001").</summary>
        public string Numero { get; set; } = string.Empty;

        /// <summary>Part numèrica correlativa del número (comptador global del període).</summary>
        public int Serie { get; set; }

        // FK → remesa d'emissió
        public int RemesaId { get; set; }
        public virtual LiquidacioRemesa? Remesa { get; set; }

        // FK → alumna
        public int AlumneId { get; set; }
        public virtual Alumne? Alumne { get; set; }

        // FK → curset
        public int CursetId { get; set; }
        public virtual Curset? Curset { get; set; }

        // Trossos del número (snapshot)
        public string? CodiServei { get; set; }
        public string? CodiPeriode { get; set; }
        public string? CodiCurset { get; set; }

        public DateTime PeriodeInici { get; set; }
        public DateTime PeriodeFi { get; set; }
        public string PeriodeEtiqueta { get; set; } = string.Empty;

        /// <summary>Nombre de sessions impartides facturades.</summary>
        public int NumSessions { get; set; }
        public decimal PreuPerSessio { get; set; }
        public decimal Import { get; set; }
        public bool Empadronat { get; set; }

        public EstatLiquidacio Estat { get; set; } = EstatLiquidacio.Emesa;

        public DateTime DataEmissio { get; set; } = DateTime.Now;
        public DateTime? DataCobrament { get; set; }
        public DateTime? DataAnullacio { get; set; }
        public string? MotiuAnullacio { get; set; }

        // ---- Snapshots per a la reimpressió ----
        public string AlumneNomComplet { get; set; } = string.Empty;
        public string? AlumneNif { get; set; }
        public string? AlumneAdreca { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public string ConcepteText { get; set; } = string.Empty;
        public string? OrdenancaText { get; set; }
        public string? DatesHorariText { get; set; }
        /// <summary>Dates de les sessions comptades, JSON (["2026-01-09", ...]).</summary>
        public string? DetallSessionsJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
