namespace AppAjuntament.Models.Api;

/// <summary>
/// Model per als registres d'APIs disponibles del cataleg SEU-e
/// </summary>
public class SeuApiRecord
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Descripcio { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool IsUtilizada { get; set; }
    public string? Observacions { get; set; }
}

/// <summary>
/// Cataleg complet de les APIs SEU-e disponibles (Santa Maria de Martorelles)
/// </summary>
public static class SeuApiCatalog
{
    public static List<SeuApiRecord> GetAllApis()
    {
        return new List<SeuApiRecord>
        {
            new() { Id = 1, Nom = "Contractes publicats en el Perfil de Contractant (servei AOC)", Descripcio = "Activitat de contractacio publica associada al perfil del contractant.", ResourceId = "7448c675-8880-464e-9980-1b92119e59c8", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Contractacio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 2, Nom = "Dades d'e-Administracio de l'ens", Descripcio = "Estat de l'administracio electronica, transparencia i bon govern.", ResourceId = "2dd896a1-3d81-4c31-92d8-ab17a1fc5199", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Entitat Digital", IsUtilizada = true, Observacions = "Actualment mostrada a la pagina principal" },
            new() { Id = 3, Nom = "Dades visites portals transparencia (historic i per anys)", Descripcio = "Visites, pagines visualitzades i us dels portals de transparencia.", ResourceId = "863fe6d7-c28d-4a5c-8c89-283069939732", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Transparencia", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 4, Nom = "Cartipas: organitzacio politica", Descripcio = "Organitzacio institucional: alcaldia, regidories i organs de govern.", ResourceId = "1dda84e8-1f08-415b-a7c7-c45b50424249", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Govern", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 5, Nom = "Convocatories de personal", Descripcio = "Convocatories de seleccio i provisio de llocs de treball.", ResourceId = "0e11c4f5-ce15-401f-b86f-f9d2604b94f6", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Personal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 6, Nom = "Actes del Ple", Descripcio = "Acords i informacio de les sessions plenaries.", ResourceId = "b5d370d0-7916-48b6-8a69-3c7fa62a1467", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Govern", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 7, Nom = "Ordenances fiscals", Descripcio = "Impostos, taxes i ordenances fiscals aprovades.", ResourceId = "dbfe266c-da98-4f4c-9296-b16f40e6b23f", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 8, Nom = "Convocatories de personal resultats", Descripcio = "Resultats dels processos selectius de personal.", ResourceId = "d9f131a1-5489-4cd7-a8ab-902b99df7968", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Personal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 9, Nom = "Registre de funcionaris habilitats de l'ens", Descripcio = "Personal funcionari habilitat inscrit.", ResourceId = "c633d5e6-b3b4-4ca1-8fc6-9e7c049c1e29", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Personal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 10, Nom = "Convocatories de subvencions i ajuts", Descripcio = "Convocatories de subvencions i ajuts, condicions i beneficiaris.", ResourceId = "b39a563c-3b96-4e64-a856-e313458f3dad", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Subvencions", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 11, Nom = "Convenis, acords, pactes de caracter funcionarial, laboral o sindical", Descripcio = "Convenis i acords laborals o sindicals aplicables.", ResourceId = "a66516f2-e82b-4217-896a-054b20f9ebcb", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Convenis", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 12, Nom = "Calendaris i padrons fiscals", Descripcio = "Periodes i dates de cobrament dels tributs periodics.", ResourceId = "59218973-d6a6-4cc1-bd3f-c9b9b851cb53", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 13, Nom = "Ordenances reguladores i reglaments", Descripcio = "Normativa general de serveis i organitzacio.", ResourceId = "4597729c-7325-4525-bada-65c74dfd8877", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Normativa", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 14, Nom = "Estatuts", Descripcio = "Estatuts i disposicions de funcionament intern.", ResourceId = "fc660dac-21cc-444c-8bef-00e13eb01520", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Normativa", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 15, Nom = "Carrecs electes", Descripcio = "Relacio de carrecs electes i adscripcio politica.", ResourceId = "eb131bb1-f521-4aeb-9004-2fea1f372e89", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Electes", IsUtilizada = true, Observacions = "Proporcionada per SeuEcService" },
            new() { Id = 16, Nom = "Activitat dels serveis del Consorci AOC (detall)", Descripcio = "Us dels serveis AOC per ens i tipus de servei.", ResourceId = "b02ad6a7-bb6c-4cc4-882f-b4a7378eeb8c", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "AOC", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 17, Nom = "Dades generals de l'ens", Descripcio = "Informacio general de l'ens (font MUNICAT).", ResourceId = "ab53cbf3-a439-4f59-a2f5-658bee1994e5", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Administracio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 18, Nom = "Tramitacio de pressupostos plantilles, i relacio de llocs de treball", Descripcio = "Pressupostos, plantilles i RLT publicades.", ResourceId = "572b66e6-a353-4973-b307-558ce10b59a4", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Pressupost", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 19, Nom = "Liquidacio del pressupost (economic i per programes)", Descripcio = "Execucio final d'ingressos i despeses per programa.", ResourceId = "5b96829f-d724-4059-a38a-abf514830558", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Pressupost", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 20, Nom = "Liquidacio del pressupost per principals ingressos tributaris", Descripcio = "Liquidacio d'impostos, taxes i preus publics.", ResourceId = "e2dbc258-07a0-4078-a872-f446acfe6c5a", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 21, Nom = "Convocatories de subvencions i ajuts (MINHAP)", Descripcio = "Convocatories de subvencions amb font del Ministeri d'Hisenda.", ResourceId = "9aa9b0b7-b8b0-48fa-9d18-69b601fe72a9", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Subvencions", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 22, Nom = "Registre de convenis de col.laboracio i cooperacio", Descripcio = "Convenis de col.laboracio i cooperacio inscrits.", ResourceId = "8747a24f-aa98-4a7e-938a-df81cc16769a", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Convenis", IsUtilizada = true, Observacions = "Mostrada a la pagina de Convenis" },
            new() { Id = 23, Nom = "Pressupost (economic i per programes)", Descripcio = "Pressupost aprovat detallat economic i per programes.", ResourceId = "595f8620-7a3c-4e38-8be2-7cffa744cd44", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Pressupost", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 24, Nom = "Concessions de subvencions i ajuts (MINHAP)", Descripcio = "Subvencions i ajuts concedits (font MINHAP).", ResourceId = "30798421-5952-4420-a769-d1121beb6534", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Subvencions", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 25, Nom = "Pressupost per principals ingressos tributaris", Descripcio = "Previsio dels principals ingressos tributaris.", ResourceId = "20b967b4-dc62-425e-8068-9c93b92d943a", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 26, Nom = "Dades d'emplenament dels portals de transparencia", Descripcio = "Percentatge d'emplenament dels items de transparencia.", ResourceId = "1a9c1ede-8486-4a00-a48f-1b3271f6115c", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Transparencia", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 27, Nom = "Cost efectiu dels serveis", Descripcio = "Cost efectiu anual dels serveis municipals.", ResourceId = "12c13cdd-03ca-48d3-92cb-f3e586e1135a", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Serveis", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 28, Nom = "Dades de contractes de Catalunya informats al Registre public de contractes", Descripcio = "Contractes informats al registre public dels darrers anys.", ResourceId = "1c15c6fa-2134-4e8b-a230-66c59278a9d6", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Contractacio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 29, Nom = "Plecs de clausules generals", Descripcio = "Plecs generals de contractacio.", ResourceId = "ed9ac827-1734-412f-a33a-4c85fa850d7e", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Contractacio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 30, Nom = "Organismes dels que forma part", Descripcio = "Organismes externs on participa l'ens.", ResourceId = "6525f5b3-6c7b-4e62-899b-392fd58b54ee", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Administracio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 31, Nom = "Organismes dependents o vinculats", Descripcio = "Entitats dependents o vinculades de l'ens.", ResourceId = "a5773993-4992-4ec0-84e2-95d31ad8101c", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Administracio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 32, Nom = "Registre d'eliminacio de documentacio", Descripcio = "Eliminacions documentals registrades al sector public.", ResourceId = "a6e1522c-8091-4efd-8e67-7f0fbf4e2a21", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Arxiu", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 33, Nom = "Tipus impositius municipals", Descripcio = "Tipus impositius i beneficis fiscals municipals.", ResourceId = "82ae0ea2-6fc6-4fd5-b944-4ef6d18717bc", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 34, Nom = "Periode Mitja de Pagament a proveidors (PMP)", Descripcio = "Indicador PMP per trimestres.", ResourceId = "eecca986-a51b-4b0e-a03b-6fc8bb71d387", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Fiscal", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 35, Nom = "Organismes que el formen", Descripcio = "Organismes que conformen l'estructura de l'ens.", ResourceId = "155cf6a1-60e8-4e53-aa72-b4cdc2d06412", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Administracio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 36, Nom = "Padro municipal d'habitants per municipi, any i sexe", Descripcio = "Poblacio per any i sexe segons padro municipal.", ResourceId = "e0be5678-0bdd-48e0-99af-05cd5404a9a5", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Poblacio", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 37, Nom = "Endeutament", Descripcio = "Evolucio de l'endeutament financer de l'ens.", ResourceId = "34db8dc5-ad5e-4bf0-83cc-537cd8671342", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Pressupost", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 38, Nom = "Compliment dels objectius d'estabilitat pressupostaria", Descripcio = "Informes de compliment d'estabilitat pressupostaria.", ResourceId = "db908d83-79d4-403e-a1c4-45d70515d111", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Pressupost", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" },
            new() { Id = 39, Nom = "Qualitat de l'aire als punts de mesurament manual de la Xarxa de Vigilancia i Previsio de la Contaminacio Atmosferica", Descripcio = "Dades diaries de contaminants atmosferics.", ResourceId = "b9965e52-440f-4dfe-9866-1fb3f09b3d7d", Url = "https://dadesobertes.seu-e.cat/api/3/action/datastore_search", Categoria = "Medi Ambient", IsUtilizada = false, Observacions = "Verificat al document SantaMariaMartorelles_APIs.md" }
        };
    }

    public static List<SeuApiRecord> GetUnusedApis()
    {
        return GetAllApis().Where(a => !a.IsUtilizada).ToList();
    }

    public static List<SeuApiRecord> GetUsedApis()
    {
        return GetAllApis().Where(a => a.IsUtilizada).ToList();
    }

    public static List<string> GetCategories()
    {
        return GetAllApis().Select(a => a.Categoria).Distinct().OrderBy(c => c).ToList();
    }

    public static List<SeuApiRecord> GetApisByCategory(string categoria)
    {
        return GetAllApis().Where(a => a.Categoria == categoria).ToList();
    }
}
