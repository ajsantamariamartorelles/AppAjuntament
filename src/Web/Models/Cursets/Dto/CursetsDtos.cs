namespace AppAjuntament.Models.Cursets.Dto
{
    // ==================== Auth ====================

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        /// <summary>"Professora" o "Controlador".</summary>
        public string Rol { get; set; } = "Professora";

        /// <summary>Id del subjecte: usuari (professora) o regidor (controlador).</summary>
        public int ProfessoraId { get; set; }

        /// <summary>Nom a mostrar a l'app ("Hola, ...").</summary>
        public string ProfessoraNom { get; set; } = string.Empty;

        /// <summary>Si cal mostrar la pantalla d'acceptació de termes i protecció de
        /// dades abans d'entrar (encara no ha acceptat la versió vigent).</summary>
        public bool CalAcceptarTermes { get; set; }
    }

    /// <summary>Petició per crear/actualitzar credencials de professora. Només accessible des de la web (usuari municipal autenticat).</summary>
    public class CrearProfessoraRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Fila del llistat de professores a la gestió web.</summary>
    public class ProfessoraDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        /// <summary>Activa = pot entrar a l'app MAUI (té el rol "Professora" i no està de baixa).</summary>
        public bool Activa { get; set; }
        /// <summary>Nombre de cursets actius assignats a la professora.</summary>
        public int NumCursets { get; set; }
    }

    /// <summary>Actualització de les dades bàsiques d'una professora existent.</summary>
    public class ActualitzarProfessoraRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>Alta/baixa d'una professora (sense esborrar l'usuari ni el seu historial).</summary>
    public class CanviarEstatProfessoraRequest
    {
        public bool Activa { get; set; }
    }

    /// <summary>Canvi de contrasenya per part del propi usuari de l'app (professora o controlador).</summary>
    public class CanviarPasswordPropiaRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNova { get; set; } = string.Empty;
    }

    /// <summary>Nova contrasenya per a una professora.</summary>
    public class ReiniciarPasswordProfessoraRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    // ==================== Tipus de curset (temàtica) ====================

    public class TipusCursetDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        /// <summary>Codi curt del servei per als números de liquidació ("PTES").</summary>
        public string? CodiLiquidacio { get; set; }
    }

    public class CrearTipusCursetRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string? CodiLiquidacio { get; set; }
    }

    /// <summary>Actualització d'una temàtica existent (nom + codi de liquidació).</summary>
    public class ActualitzarTipusCursetRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string? CodiLiquidacio { get; set; }
    }

    // ==================== Cursets ====================

    public class CursetDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Titol { get; set; } = string.Empty;
        public int TipusCursetId { get; set; }
        public string TipusCursetNom { get; set; } = string.Empty;
        public int ProfessoraId { get; set; }
        public string ProfessoraNom { get; set; } = string.Empty;
        public int? RegidorId { get; set; }
        public string? RegidorNom { get; set; }
        public DayOfWeek? DiaSetmana { get; set; }
        public TimeSpan? HoraInici { get; set; }
        public TimeSpan? HoraFi { get; set; }
        public bool Actiu { get; set; }
        public int NumAlumnes { get; set; }
        /// <summary>Nombre màxim de places (null = sense límit).</summary>
        public int? MaxPlaces { get; set; }

        /// <summary>Codi curt del curset per als números de liquidació ("DVT").</summary>
        public string? CodiLiquidacio { get; set; }
        /// <summary>Cadència de liquidació per defecte ("Trimestral" / "Mensual" / "Setmanal").</summary>
        public string? CadenciaLiquidacio { get; set; }
        /// <summary>Ordenança fiscal i/o tarifa aplicable (surt a l'autoliquidació).</summary>
        public string? OrdenancaLiquidacio { get; set; }

        /// <summary>Període d'inscripció (dates naturals). Fora del període es considera tancada.</summary>
        public DateTime? InscripcioInici { get; set; }
        public DateTime? InscripcioFi { get; set; }
        /// <summary>Enllaç extern d'inscripció (formulari, tràmit de la Seu…).</summary>
        public string? UrlInscripcio { get; set; }

        /// <summary>Preu per classe impartida (no per assistència). 0 = curset sense cost.</summary>
        public decimal PreuPerSessioEmpadronat { get; set; }
        public decimal PreuPerSessioNoEmpadronat { get; set; }
    }

    public class CrearCursetRequest
    {
        public string Nom { get; set; } = string.Empty;
        public int TipusCursetId { get; set; }
        public int ProfessoraId { get; set; }
        /// <summary>Regidor responsable del curset (opcional).</summary>
        public int? RegidorId { get; set; }
        public DayOfWeek? DiaSetmana { get; set; }
        public TimeSpan? HoraInici { get; set; }
        public TimeSpan? HoraFi { get; set; }
        public decimal PreuPerSessioEmpadronat { get; set; }
        public decimal PreuPerSessioNoEmpadronat { get; set; }
        /// <summary>Nombre màxim de places (null = sense límit).</summary>
        public int? MaxPlaces { get; set; }
        /// <summary>Codi curt del curset per als números de liquidació ("DVT").</summary>
        public string? CodiLiquidacio { get; set; }
        /// <summary>Cadència de liquidació per defecte ("Trimestral" / "Mensual" / "Setmanal").</summary>
        public string? CadenciaLiquidacio { get; set; }
        /// <summary>Ordenança fiscal i/o tarifa aplicable (surt a l'autoliquidació).</summary>
        public string? OrdenancaLiquidacio { get; set; }

        /// <summary>Període d'inscripció (dates naturals). Fora del període es considera tancada.</summary>
        public DateTime? InscripcioInici { get; set; }
        public DateTime? InscripcioFi { get; set; }
        /// <summary>Enllaç extern d'inscripció (formulari, tràmit de la Seu…).</summary>
        public string? UrlInscripcio { get; set; }
    }

    // ==================== Cursos (pàgina pública) ====================

    /// <summary>Informació d'un curs per a la pàgina pública: només el bàsic per decidir si t'hi apuntes.</summary>
    public class CursetPublicDto
    {
        public int Id { get; set; }
        public string Tematica { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Titol { get; set; } = string.Empty;
        public DayOfWeek? DiaSetmana { get; set; }
        public TimeSpan? HoraInici { get; set; }
        public TimeSpan? HoraFi { get; set; }
        public decimal PreuPerSessioEmpadronat { get; set; }
        public decimal PreuPerSessioNoEmpadronat { get; set; }
        /// <summary>Places totals ofertes (null = sense límit definit). Informatiu: l'adjudicació
        /// no és per ordre d'inscripció, sinó per sorteig si hi ha més sol·licituds que places.</summary>
        public int? PlacesTotals { get; set; }

        /// <summary>Estat de la inscripció segons les dates i la data d'avui:
        /// "Oberta", "Properament", "Tancada" o "SenseDates".</summary>
        public string EstatInscripcio { get; set; } = "SenseDates";
        public DateTime? InscripcioInici { get; set; }
        public DateTime? InscripcioFi { get; set; }
        /// <summary>Enllaç extern per inscriure-s'hi (només s'ha de mostrar si la inscripció és oberta).</summary>
        public string? UrlInscripcio { get; set; }
    }

    /// <summary>Actualització d'un curset existent (mateixos camps que la creació).</summary>
    public class ActualitzarCursetRequest : CrearCursetRequest
    {
    }

    // ==================== Regidors / controladors ====================

    /// <summary>Regidor del mandat actual, per assignar-lo com a responsable d'un curset.</summary>
    public class RegidorVigentDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Carrec { get; set; }
        public string? Email { get; set; }
        /// <summary>Té contrasenya → pot entrar a l'app com a controlador.</summary>
        public bool TeAcces { get; set; }
        /// <summary>Nombre de cursets actius on és el regidor responsable.</summary>
        public int NumCursets { get; set; }
    }

    /// <summary>Dona o actualitza l'accés a l'app d'un regidor (controlador de cursets).</summary>
    public class DonarAccesControladorRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Alta/baixa d'un curset.</summary>
    public class CanviarEstatCursetRequest
    {
        public bool Actiu { get; set; }
    }

    // ==================== Alumnes ====================

    public class AlumneDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public bool Actiu { get; set; }
        public bool Empadronat { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>Alumne amb tots els camps editables per a la gestió web.</summary>
    public class AlumneAdminDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Cognoms { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Poblacio { get; set; }
        public string? Notes { get; set; }
        public bool Actiu { get; set; }
        public bool Empadronat { get; set; }
        public List<int> CursetIds { get; set; } = new();
        public string CursetsResum { get; set; } = string.Empty;
        /// <summary>Camps que falten per poder emetre una liquidació (buit = complet).</summary>
        public List<string> DadesLiquidacioFalten { get; set; } = new();
        /// <summary>Deute pendent: suma de les liquidacions en estat Emesa.</summary>
        public decimal Deute { get; set; }
    }

    public class CrearAlumneRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string? Cognoms { get; set; }
        public string? Dni { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Poblacio { get; set; }
        public string? Notes { get; set; }
        public bool Empadronat { get; set; }
        public List<int> CursetIds { get; set; } = new();

        /// <summary>Confirmació del personal que la persona ha signat l'autorització de
        /// tractament de dades. Només cal si es crea un Tercer nou (si el DNI ja existeix
        /// a Tercers, es reutilitza i no cal tornar-la a demanar).</summary>
        public bool AutoritzacioTractamentSignada { get; set; }
    }

    /// <summary>Actualització d'un alumne existent. Si <see cref="CursetIds"/> és null, no es toquen les inscripcions.</summary>
    public class ActualitzarAlumneRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string? Cognoms { get; set; }
        public string? Dni { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Poblacio { get; set; }
        public string? Notes { get; set; }
        public bool Empadronat { get; set; }
        /// <summary>Inscripcions desitjades (sincronització "posa'm exactament aquestes"). null = no tocar-les.</summary>
        public List<int>? CursetIds { get; set; }
    }

    /// <summary>Alta/baixa d'un alumne.</summary>
    public class CanviarEstatAlumneRequest
    {
        public bool Actiu { get; set; }
    }

    // ==================== Fitxa d'alumne (gestió web) ====================

    /// <summary>Fitxa completa d'un alumne: dades, inscripcions i liquidacions.</summary>
    public class AlumneFitxaDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Cognoms { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Poblacio { get; set; }
        public string? Notes { get; set; }
        public bool Actiu { get; set; }
        public bool Empadronat { get; set; }
        public DateTime DataAlta { get; set; }

        /// <summary>Deute pendent: suma de liquidacions en estat Emesa.</summary>
        public decimal Deute { get; set; }
        /// <summary>Camps que falten per poder emetre una liquidació (buit = complet).</summary>
        public List<string> DadesLiquidacioFalten { get; set; } = new();

        public List<InscripcioFitxaDto> Inscripcions { get; set; } = new();
        public List<LiquidacioFitxaDto> Liquidacions { get; set; } = new();
    }

    /// <summary>Una inscripció de l'alumne a un curset, per a la fitxa.</summary>
    public class InscripcioFitxaDto
    {
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public string? DiaHora { get; set; }
        public string? ProfessoraNom { get; set; }
        /// <summary>"Sollicitada" | "Admesa" | "LlistaEspera" | "Baixa" | "Rebutjada".</summary>
        public string Estat { get; set; } = string.Empty;
        public int? OrdreLlistaEspera { get; set; }
        /// <summary>Total d'inscripcions en llista d'espera d'aquest curset (per mostrar "2n de 7").</summary>
        public int TotalLlistaEspera { get; set; }
        /// <summary>Estat del període d'inscripció del curset: "Oberta" | "Properament" | "Tancada" | "SenseDates".</summary>
        public string EstatPeriode { get; set; } = "SenseDates";
        public DateTime? DataSollicitud { get; set; }
        public DateTime DataAlta { get; set; }
        public DateTime? DataBaixa { get; set; }
        public string? Origen { get; set; }
    }

    /// <summary>Una liquidació de l'alumne, per a la fitxa.</summary>
    public class LiquidacioFitxaDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string PeriodeEtiqueta { get; set; } = string.Empty;
        public string CursetTitol { get; set; } = string.Empty;
        public decimal Import { get; set; }
        /// <summary>"Emesa" | "Cobrada" | "Anullada".</summary>
        public string Estat { get; set; } = string.Empty;
        public DateTime? DataCobrament { get; set; }
    }

    /// <summary>Canvi d'estat d'una inscripció concreta des de la fitxa de l'alumne.</summary>
    public class CanviarEstatInscripcioRequest
    {
        public int CursetId { get; set; }
        /// <summary>"Admesa" | "LlistaEspera" | "Baixa" | "Rebutjada".</summary>
        public string NouEstat { get; set; } = string.Empty;
    }

    /// <summary>Alta manual d'una inscripció (personal) des de la fitxa de l'alumne.</summary>
    public class AfegirInscripcioRequest
    {
        public int CursetId { get; set; }
    }

    // ==================== Sol·licituds d'inscripció / sorteig ====================

    /// <summary>Sol·licitud d'inscripció des del formulari públic de /cursos.</summary>
    public class SollicitudInscripcioPublicaRequest
    {
        public int CursetId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Cognoms { get; set; }
        public string? Dni { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Poblacio { get; set; }
        /// <summary>L'interessat es declara empadronat al municipi (el personal ho confirma en admetre).</summary>
        public bool DeclaraEmpadronat { get; set; }
        /// <summary>Consentiment del tractament de dades (RGPD). Obligatori.</summary>
        public bool ConsentimentRgpd { get; set; }
        /// <summary>Camp trampa anti-bot: ha d'arribar buit.</summary>
        public string? Honeypot { get; set; }
    }

    /// <summary>Una sol·licitud pendent d'un curset (per al panell de sorteig).</summary>
    public class SollicitudPendentDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public bool Empadronat { get; set; }
        /// <summary>"Sollicitada" | "LlistaEspera".</summary>
        public string Estat { get; set; } = string.Empty;
        public int? OrdreLlistaEspera { get; set; }
        public DateTime? DataSollicitud { get; set; }
        public string? Origen { get; set; }
    }

    /// <summary>Estat d'un curset de cara al sorteig: places, admesos i sol·licituds pendents.</summary>
    public class SorteigEstatDto
    {
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        /// <summary>Places totals (null = sense límit; s'admeten totes les sol·licituds).</summary>
        public int? PlacesTotals { get; set; }
        public int Admesos { get; set; }
        /// <summary>Places lliures ara mateix (null si no hi ha límit).</summary>
        public int? PlacesLliures { get; set; }
        public int Sollicituds { get; set; }
        public int EnLlistaEspera { get; set; }
        /// <summary>Quantes s'admetrien si es fes el sorteig ara.</summary>
        public int NAdmetria { get; set; }
        /// <summary>Quantes anirien a la llista d'espera.</summary>
        public int NAEspera { get; set; }
        /// <summary>Número de tall de l'últim sorteig fet (null si no se n'ha fet cap).</summary>
        public int? UltimSorteigNumero { get; set; }
        public DateTime? UltimSorteigData { get; set; }
        public List<SollicitudPendentDto> Pendents { get; set; } = new();
    }

    /// <summary>Resultat d'un sorteig.</summary>
    public class SorteigResultDto
    {
        /// <summary>Número de tall sortejat (posició 1..TotalInscrits a partir de la qual s'adjudiquen les places).</summary>
        public int Numero { get; set; }
        /// <summary>Total de sol·licitants que entraven al sorteig.</summary>
        public int TotalInscrits { get; set; }
        public int NAdmesos { get; set; }
        public int NAEspera { get; set; }
        public List<string> Admesos { get; set; } = new();
        public List<string> AEspera { get; set; } = new();
    }

    // ==================== Sessions / Assistència ====================

    public class AssistenciaAlumneDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public bool Present { get; set; } = true;
        public string? Nota { get; set; }
    }

    public class SessioObertaDto
    {
        public int SessioId { get; set; }
        public int CursetId { get; set; }
        public DateTime Data { get; set; }
        public List<AssistenciaAlumneDto> Alumnes { get; set; } = new();
    }

    public class GuardarAssistenciaRequest
    {
        public string? NotaSessio { get; set; }
        public List<AssistenciaAlumneDto> Alumnes { get; set; } = new();
    }

    // ==================== Consulta d'assistències (gestió web) ====================

    /// <summary>Una sessió d'un curset amb el detall de qui hi va assistir.</summary>
    public class SessioAssistenciaDto
    {
        public int SessioId { get; set; }
        public int CursetId { get; set; }
        public string CursetNom { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        /// <summary>"Oberta" o "Tancada".</summary>
        public string Estat { get; set; } = string.Empty;
        public string? NotaSessio { get; set; }
        public int NumPresents { get; set; }
        public int NumTotal { get; set; }
        /// <summary>La sessió cau dins el període d'una liquidació no anul·lada: no es pot eliminar.</summary>
        public bool Liquidada { get; set; }
        public List<AssistenciaAlumneDto> Alumnes { get; set; } = new();
    }

    /// <summary>Crea (des de la web) una llista d'assistència d'una data passada o d'avui.</summary>
    public class CrearLlistaAssistenciaRequest
    {
        public DateTime Data { get; set; }
        public string? NotaSessio { get; set; }
        public List<AssistenciaAlumneDto> Alumnes { get; set; } = new();
    }

    /// <summary>Una fila d'assistència d'un alumne a una sessió concreta.</summary>
    public class AssistenciaPerAlumneDto
    {
        public int SessioId { get; set; }
        public int CursetId { get; set; }
        public string CursetNom { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string Estat { get; set; } = string.Empty;
        public bool Present { get; set; }
        public string? Nota { get; set; }
    }

    /// <summary>Historial d'assistència d'un alumne a totes les seves sessions.</summary>
    public class ResumAssistenciaAlumneDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int TotalPresents { get; set; }
        public List<AssistenciaPerAlumneDto> Sessions { get; set; } = new();
    }

    // ==================== Absències consecutives ====================

    /// <summary>
    /// Una inscripció activa (Admesa) amb absències consecutives: sessions tancades més
    /// recents en què l'alumne ha faltat seguides, sense cap presència pel mig.
    /// </summary>
    public class AbsenciaAlumneDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefon { get; set; }
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        /// <summary>Nombre de sessions tancades consecutives (les més recents) on ha faltat.</summary>
        public int AbsenciesConsecutives { get; set; }
        public DateTime DataUltimaSessio { get; set; }
        /// <summary>Última sessió a què va assistir (null si no hi ha cap presència registrada).</summary>
        public DateTime? DataUltimaPresencia { get; set; }
    }

    /// <summary>Una inscripció (alumne × curset) seleccionada per donar de baixa.</summary>
    public class BaixaPerAbsenciesItem
    {
        public int AlumneId { get; set; }
        public int CursetId { get; set; }
    }

    /// <summary>Baixa massiva per absències no justificades: canvia l'estat de les inscripcions
    /// seleccionades a Baixa i avisa per correu els alumnes que en tinguin.</summary>
    public class DonarBaixaPerAbsenciesRequest
    {
        public List<BaixaPerAbsenciesItem> Seleccio { get; set; } = new();
        /// <summary>Text addicional pel correu (opcional).</summary>
        public string? Missatge { get; set; }
    }

    /// <summary>Resultat d'una baixa massiva per absències.</summary>
    public class DonarBaixaPerAbsenciesResultDto
    {
        public int NumBaixes { get; set; }
        public int NumEmailsEnviats { get; set; }
        public int NumSenseEmail { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Configuració del correu de baixa per absències: connexió SMTP i plantilla del
    /// missatge. Editable des de la web sense necessitat de publicar.
    /// Tokens admesos a Assumpte i CosHtml: {NOM}, {CURSET}, {INSTITUCIO}, {MISSATGE}.
    /// </summary>
    public class EmailAbsenciesConfigDto
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool SmtpSsl { get; set; } = true;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string RemitentEmail { get; set; } = string.Empty;
        public string RemitentNom { get; set; } = string.Empty;
        public string Assumpte { get; set; } = string.Empty;
        public string CosHtml { get; set; } = string.Empty;
    }

    // ==================== Liquidació (llistat de deute) ====================

    /// <summary>
    /// Import degut per una alumna en un període: nre. de sessions IMPARTIDES
    /// (Estat=Tancada) del curset mentre hi era apuntada × preu segons si és
    /// empadronada o no. No depèn de si ella hi va assistir personalment.
    /// </summary>
    public class LiquidacioAlumnaDto
    {
        public int AlumneId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public bool Empadronat { get; set; }
        public int NumSessions { get; set; }
        public decimal PreuPerSessio { get; set; }
        public decimal ImportTotal { get; set; }
    }

    public class LiquidacioCursetDto
    {
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public DateTime DataInici { get; set; }
        public DateTime DataFi { get; set; }
        public int NumSessionsFetes { get; set; }
        public List<LiquidacioAlumnaDto> Alumnes { get; set; } = new();
        public decimal ImportTotal { get; set; }
    }

    // ==================== Liquidacions registrades (remeses + PDF) ====================

    /// <summary>Període a liquidar. Segons <see cref="Cadencia"/>, <see cref="Periode"/> és
    /// el trimestre (1-4), el mes (1-12) o la setmana ISO; amb cadència "Lliure" s'usen
    /// <see cref="DataInici"/>/<see cref="DataFi"/>.</summary>
    public class PeriodeLiquidacioRequest
    {
        /// <summary>"Trimestral" | "Mensual" | "Setmanal" | "Lliure".</summary>
        public string Cadencia { get; set; } = "Trimestral";
        public int Any { get; set; }
        public int? Periode { get; set; }
        public DateTime? DataInici { get; set; }
        public DateTime? DataFi { get; set; }
        /// <summary>Cursets a incloure. Buit/null = tots els actius amb sessions al període.</summary>
        public List<int>? CursetIds { get; set; }
    }

    /// <summary>Una línia de la previsualització: què cobrarà una alumna en un curset.</summary>
    public class LiquidacioLiniaPreviewDto
    {
        public int AlumneId { get; set; }
        public string AlumneNom { get; set; } = string.Empty;
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public bool Empadronat { get; set; }
        public int NumSessions { get; set; }
        public decimal PreuPerSessio { get; set; }
        public decimal Import { get; set; }
        /// <summary>Ja hi ha una liquidació no anul·lada per aquesta (alumna × curset) en un període solapat.</summary>
        public bool JaLiquidada { get; set; }
        public string? NumeroExistent { get; set; }
        public string? EstatExistent { get; set; }
        /// <summary>L'alumna té totes les dades necessàries per emetre l'autoliquidació.</summary>
        public bool DadesCompletes { get; set; } = true;
        /// <summary>Camps que falten (p. ex. "NIF/DNI, població").</summary>
        public string? DadesFalten { get; set; }
        /// <summary>El curset té ordenança / tarifa (pròpia o per defecte).</summary>
        public bool CursetOrdenancaOk { get; set; } = true;
    }

    /// <summary>Alumna que no es pot liquidar perquè li falten dades bàsiques.</summary>
    public class AlumnaIncompletaDto
    {
        public int AlumneId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string DadesFalten { get; set; } = string.Empty;
    }

    public class PreviewRemesaDto
    {
        public string Cadencia { get; set; } = string.Empty;
        public string CodiPeriode { get; set; } = string.Empty;
        public string PeriodeEtiqueta { get; set; } = string.Empty;
        public DateTime DataInici { get; set; }
        public DateTime DataFi { get; set; }
        public List<LiquidacioLiniaPreviewDto> Linies { get; set; } = new();
        public int NumNoves { get; set; }
        public int NumJaLiquidades { get; set; }
        public int NumBloquejades { get; set; }
        public decimal ImportNoves { get; set; }
        /// <summary>Avisos (p. ex. cursets sense codi de liquidació).</summary>
        public List<string> Avisos { get; set; } = new();
        /// <summary>Alumnes que no es poden liquidar per falta de dades.</summary>
        public List<AlumnaIncompletaDto> AlumnesIncompletes { get; set; } = new();
        /// <summary>Cursets que no es poden liquidar perquè no tenen ordenança / tarifa.</summary>
        public List<string> CursetsSenseOrdenanca { get; set; } = new();
    }

    public class EmetreRemesaRequest : PeriodeLiquidacioRequest
    {
        public string Descripcio { get; set; } = string.Empty;
        /// <summary>"NomesNoves" (per defecte) | "ReferTot" (anul·la les existents solapades i re-emet).</summary>
        public string Mode { get; set; } = "NomesNoves";
    }

    public class RemesaDto
    {
        public int Id { get; set; }
        public string Descripcio { get; set; } = string.Empty;
        public string CodiPeriode { get; set; } = string.Empty;
        public string PeriodeEtiqueta { get; set; } = string.Empty;
        public string Expedient { get; set; } = string.Empty;
        public DateTime DataCreacio { get; set; }
        public int NumLiquidacions { get; set; }
        public decimal ImportTotal { get; set; }
    }

    public class LiquidacioDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int AlumneId { get; set; }
        public string AlumneNom { get; set; } = string.Empty;
        public int CursetId { get; set; }
        public string CursetTitol { get; set; } = string.Empty;
        public int NumSessions { get; set; }
        public decimal Import { get; set; }
        public string Estat { get; set; } = string.Empty;
        public DateTime DataEmissio { get; set; }
        public DateTime? DataCobrament { get; set; }
        public DateTime? DataAnullacio { get; set; }
        public string? MotiuAnullacio { get; set; }
    }

    /// <summary>Resum per alumna dins una remesa (per als botons de PDF consolidat).</summary>
    public class AlumnaResumRemesaDto
    {
        public int AlumneId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int NumFulls { get; set; }
        public decimal Import { get; set; }
    }

    public class RemesaDetallDto
    {
        public RemesaDto Remesa { get; set; } = new();
        public List<LiquidacioDto> Liquidacions { get; set; } = new();
        public List<AlumnaResumRemesaDto> Alumnes { get; set; } = new();
    }

    /// <summary>Resultat d'emetre una remesa.</summary>
    public class EmetreRemesaResultDto
    {
        public int RemesaId { get; set; }
        public int NumEmeses { get; set; }
        public int NumAnullades { get; set; }
        public int NumOmeses { get; set; }
        /// <summary>Línies saltades per falta de dades (alumna o ordenança del curset).</summary>
        public int NumBloquejades { get; set; }
        public decimal ImportTotal { get; set; }
        public List<AlumnaIncompletaDto> AlumnesIncompletes { get; set; } = new();
        public List<string> CursetsSenseOrdenanca { get; set; } = new();
    }

    public class MarcarCobradaRequest
    {
        public DateTime Data { get; set; } = DateTime.Today;
    }

    public class AnullarLiquidacioRequest
    {
        public string Motiu { get; set; } = string.Empty;
    }

    /// <summary>Textos configurables de la plantilla d'autoliquidació.</summary>
    public class LiquidacioConfigDto
    {
        public string ExpedientPatro { get; set; } = string.Empty;
        public string OrdenancaTarifa { get; set; } = string.Empty;
        public string ConceptePatro { get; set; } = string.Empty;
        public string EntitatsColaboradoresText { get; set; } = string.Empty;
        public string MitjansPagamentText { get; set; } = string.Empty;
        public string OficinaCobratoriaText { get; set; } = string.Empty;
        public string TextRecursos { get; set; } = string.Empty;
        public string TextImportant { get; set; } = string.Empty;
        public string TextTerminis { get; set; } = string.Empty;
        public string CapcaleraMunicipi { get; set; } = string.Empty;
    }
}
