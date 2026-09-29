namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Un curset (p.ex. "Pilates - Dilluns matí"): té una temàtica (TipusCurset),
    /// una professora fixa i un horari habitual (dia de la setmana + hora), tot i
    /// que les sessions es creen una a una.
    /// </summary>
    public class Curset
    {
        public int Id { get; set; }

        /// <summary>Nom propi del curset (p.ex. "Dilluns matí", "Grup avançat").</summary>
        public string Nom { get; set; } = string.Empty;

        // FK → TipusCurset (temàtica: Pilates, Ioga, Zumba...)
        public int TipusCursetId { get; set; }
        public virtual TipusCurset? TipusCurset { get; set; }

        // FK → Usuari (professora fixa del curset)
        public int ProfessoraId { get; set; }
        public virtual AppAjuntament.Models.Base.Usuari.Usuari? Professora { get; set; }

        // FK → Regidor (regidor responsable del curset; pot supervisar-lo des de
        // l'app en mode lectura). Opcional.
        public int? RegidorId { get; set; }
        public virtual AppAjuntament.Models.Base.Regidor.Regidor? Regidor { get; set; }

        /// <summary>0=Diumenge..6=Dissabte, com System.DayOfWeek.</summary>
        public DayOfWeek? DiaSetmana { get; set; }
        public TimeSpan? HoraInici { get; set; }
        public TimeSpan? HoraFi { get; set; }

        public bool Actiu { get; set; } = true;

        /// <summary>Nombre màxim de places del curset. null = sense límit definit.</summary>
        public int? MaxPlaces { get; set; }

        /// <summary>Període d'inscripció (dates naturals). Fora del període la inscripció es
        /// considera tancada; el sistema l'obre i la tanca sol segons la data d'avui. null = sense definir.</summary>
        public DateTime? InscripcioInici { get; set; }
        public DateTime? InscripcioFi { get; set; }

        /// <summary>Enllaç extern d'inscripció d'aquest curset (formulari, tràmit de la Seu…).</summary>
        public string? UrlInscripcio { get; set; }

        /// <summary>Número de tall de l'últim sorteig (posició 1..N a partir de la qual s'adjudiquen les places).</summary>
        public int? SorteigNumero { get; set; }
        /// <summary>Data de l'últim sorteig fet.</summary>
        public DateTime? SorteigData { get; set; }

        /// <summary>Codi curt del curset per als números de liquidació (p. ex. "DVT" = Divendres tarda).</summary>
        public string? CodiLiquidacio { get; set; }

        /// <summary>Cadència de liquidació per defecte: "Trimestral", "Mensual" o "Setmanal".</summary>
        public string? CadenciaLiquidacio { get; set; }

        /// <summary>Ordenança fiscal i/o tarifa aplicable, tal com surt a l'autoliquidació.</summary>
        public string? OrdenancaLiquidacio { get; set; }

        /// <summary>
        /// Preu per classe impartida (no per assistència: si la professora ha fet
        /// classe, es factura a totes les alumnes apuntades, hi hagin anat o no).
        /// 0 = curset sense cost. Import diferent segons si l'alumna és
        /// empadronada al municipi o no.
        /// </summary>
        public decimal PreuPerSessioEmpadronat { get; set; }
        public decimal PreuPerSessioNoEmpadronat { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Títol mostrat a la UI: temàtica + nom propi (p.ex. "Pilates - Dilluns matí").</summary>
        public string Titol => TipusCurset != null ? $"{TipusCurset.Nom} - {Nom}" : Nom;

        // Collections
        public virtual ICollection<AlumneCurset> Alumnes { get; set; } = new List<AlumneCurset>();
        public virtual ICollection<Sessio> Sessions { get; set; } = new List<Sessio>();
    }
}
