namespace AppAjuntament.Models
{
    public class AgreementViewModel
    {
        public int ANY_SIGNATURA { get; set; }
        public string TITOL_CONVENI { get; set; } = string.Empty;
        public string MATERIA { get; set; } = string.Empty;
        public int CODI_SECCIO { get; set; }
        public string SECCIO { get; set; } = string.Empty;
        public string NUMERO_CONVENI_DEFINITIU { get; set; } = string.Empty;
        public string CONVENIS_RELACIONATS { get; set; } = string.Empty;
        public DateTime DATA_SIGNATURA { get; set; }
        public DateTime DATA_VIGENCIA { get; set; }
        public string DURADA { get; set; } = string.Empty;
        public string VIGENT { get; set; } = string.Empty;
        public string PRORROGABLE { get; set; } = string.Empty;
        public string OBJECTE { get; set; } = string.Empty;
        public string DRETS_I_OBLIGACIONS { get; set; } = string.Empty;
        public string COMPLIMENT_I_EXECUCIO { get; set; } = string.Empty;
        public string ORGANISMES_SIGNANTS_GENERALITAT { get; set; } = string.Empty;
        public string ORGANISMES_SIGNANTS_ENS_LOCALS { get; set; } = string.Empty;
        public long CODI_ENS { get; set; }
        public string ALTRES_ORGANISMES_SIGNANTS { get; set; } = string.Empty;
        public decimal APORTACIONS_PREVISTES_GENERALITAT { get; set; }
        public decimal APORTACIONS_PREVISTES_ENS_LOCALS { get; set; }
        public decimal APORTACIONS_PREVISTES_ALTRES_ORGANISMES { get; set; }
        public decimal TOTAL_APORTACIONS_PREVISTES { get; set; }
        public string PDF_CONVENI { get; set; } = string.Empty;
        public string ALTRES_DOCUMENTS { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsLocal { get; set; }
        public int? Id { get; set; } // For locals
    }
}