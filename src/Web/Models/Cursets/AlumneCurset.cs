namespace AppAjuntament.Models.Cursets
{
    /// <summary>Inscripció d'un alumne a un curset (relació N:M).</summary>
    public class AlumneCurset
    {
        public int AlumneId { get; set; }
        public virtual Alumne? Alumne { get; set; }

        public int CursetId { get; set; }
        public virtual Curset? Curset { get; set; }

        /// <summary>Estat de la inscripció (sol·licitada / admesa / llista d'espera / baixa / rebutjada).</summary>
        public EstatInscripcio Estat { get; set; } = EstatInscripcio.Sollicitada;

        /// <summary>Posició a la llista d'espera (1..N), assignada pel sorteig. null si no hi és.</summary>
        public int? OrdreLlistaEspera { get; set; }

        /// <summary>Data en què va entrar la sol·licitud (des de la web o alta manual).</summary>
        public DateTime? DataSollicitud { get; set; }

        /// <summary>Origen de la inscripció: "Web" (formulari públic) o "Manual" (personal). null = files antigues.</summary>
        public string? Origen { get; set; }

        /// <summary>Data d'alta al curset (quan se li adjudica plaça). Delimita quines sessions se li facturen.</summary>
        public DateTime DataAlta { get; set; } = DateTime.Now;

        /// <summary>Data en què va deixar el curset (null si encara hi és). Delimita quines sessions se li facturen.</summary>
        public DateTime? DataBaixa { get; set; }
    }
}
