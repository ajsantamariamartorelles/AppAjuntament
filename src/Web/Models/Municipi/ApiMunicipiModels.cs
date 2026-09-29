namespace AppAjuntament.Models.Municipi
{
    public class ApiGrupComarca
    {
        public string? comarca_codi { get; set; }
        public string? comarca_nom { get; set; }
    }

    public class ApiGrupProvincia
    {
        public string? provincia_codi { get; set; }
        public string? provincia_nom { get; set; }
    }

    public class ApiGrupAjuntament
    {
        public string? adreca_completa { get; set; }
        public string? adreca { get; set; }
        public string? codi_postal { get; set; }
        public string? localitzacio { get; set; }
        public string? telefon_contacte { get; set; }
        public string? fax { get; set; }
        public string? email { get; set; }
        public string? url_general { get; set; }
        public string? cif { get; set; }
    }

    public class ApiMunicipiElement
    {
        public string? ine { get; set; }
        public string? municipi_nom { get; set; }
        public string? municipi_nom_curt { get; set; }
        public string? municipi_article { get; set; }
        public string? municipi_transliterat { get; set; }
        public string? municipi_curt_transliterat { get; set; }
        public string? centre_municipal { get; set; }
        public ApiGrupComarca? grup_comarca { get; set; }
        public ApiGrupProvincia? grup_provincia { get; set; }
        public ApiGrupAjuntament? grup_ajuntament { get; set; }
        public string? municipi_escut { get; set; }
        public string? municipi_bandera { get; set; }
        public string? municipi_vista { get; set; }
        public string? ine6 { get; set; }
        public string? nom_dbpedia { get; set; }
        public string? nombre_habitants { get; set; }
        public string? extensio { get; set; }
        public string? altitud { get; set; }
    }

    public class EntitatDigitalRecord
    {
        public int _id { get; set; }
        public string? DATA_ACTUALITZACIO { get; set; }
        public string? URL_WEB { get; set; }
        public string? URL_TRAMITS { get; set; }
        public string? URL_CATALEG_SERVEIS { get; set; }
        public string? URL_CARPETA_CIUTADA { get; set; }
        public string? URL_SEU_ELECTRONICA { get; set; }
        public string? URL_TAULER_ELECTRONIC { get; set; }
        public string? URL_PERFIL_CONTRACTANT { get; set; }
        public string? DESCRIPCIO_PERFIL_CONTRACTANT { get; set; }
        public string? URL_VISOR_CARTOGRAFIC { get; set; }
        public string? URL_VISOR_URBANISME { get; set; }
        public string? URL_NOTIFICACIO_ELECTRONICA { get; set; }
        public string? URL_FACTURA_ELECTRONICA { get; set; }
        public string? TE_NOTIFICACIO_ELECTRONICA { get; set; }
        public string? TE_FACTURA_ELECTRONICA { get; set; }
        public string? TE_INSTANCIA_GENERICA { get; set; }
        public string? TE_ORDENANCA_REGULADORA { get; set; }
        public string? ES_ENTITAT_REGISTRE_TCAT { get; set; }
        public string? ES_ENTITAT_REGISTRE_IDCAT { get; set; }
        public string? URL_REPRESENTA { get; set; }
        public string? URL_INSTANCIA_GENERICA { get; set; }
        public string? URL_QUEIXES_SUGGERIMENTS { get; set; }
        public string? URL_SAIP { get; set; }
        public long CODI_ENS { get; set; }
        public string? NOM_ENS { get; set; }
    }

    public class ApiMunicipiResponse
    {
        public int total { get; set; }
        public List<ApiMunicipiElement>? elements { get; set; }
    }
}
