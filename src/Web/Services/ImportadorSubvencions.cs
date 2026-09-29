using AppAjuntament.Models;
using AppAjuntament.Models.Subvencions;
using AreaModel = AppAjuntament.Models.Base.Comarca.Area;
using EnsModel = AppAjuntament.Models.Base.Ens.Ens;
using RegidorModel = AppAjuntament.Models.Base.Regidor.Regidor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Data;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace AppAjuntament.Services
{
    public class ImportadorSubvencions
    {
        private readonly IDbContextFactory<GestorSubvencionsContext> _contextFactory;
        private readonly DatabaseLoggerService _dbLogger;

        // Helper to remove diacritics/accents from a string
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public ImportadorSubvencions(IDbContextFactory<GestorSubvencionsContext> contextFactory, DatabaseLoggerService dbLogger)
        {
            _contextFactory = contextFactory;
            _dbLogger = dbLogger;
        }

        public async Task<ResultatImportacio> ImportarCSV(string rutaFitxer, int? anySeleccionatId)
        {
            var resultat = new ResultatImportacio();

            await _dbLogger.LogInformationAsync($"Iniciant importació de fitxer: {Path.GetFileName(rutaFitxer)}", "ImportarCSV", $"Any={anySeleccionatId}");

            try
            {
                // Verificar que el fitxer existeix
                if (!File.Exists(rutaFitxer))
                {
                    await _dbLogger.LogWarningAsync($"Fitxer no trobat: {rutaFitxer}", "ImportarCSV");
                    resultat.Errors.Add($"El fitxer no existeix: {rutaFitxer}");
                    return resultat;
                }

                await _dbLogger.LogDebugAsync($"Processant fitxer: {Path.GetFileName(rutaFitxer)}, Extensió: {Path.GetExtension(rutaFitxer)}", "ImportarCSV");

                // Convertir a DataTable segons el format
                DataTable dataTable;
                var extensio = Path.GetExtension(rutaFitxer).ToLowerInvariant();
                if (extensio == ".xlsx" || extensio == ".xls")
                {
                    dataTable = await ConvertirExcelADataTable(rutaFitxer, resultat);
                    if (resultat.TéErrors)
                        return resultat;
                }
                else
                {
                    dataTable = await ConvertirCSVADataTable(rutaFitxer, resultat);
                    if (resultat.TéErrors)
                        return resultat;
                }

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    resultat.Errors.Add("No s'han trobat dades per processar");
                    return resultat;
                }

                // Flexible column mapping
                string[] columnesEsperades = new string[] {
                    "Regidor", "Àrea", "Organisme", "Subvenció", "Actuació", "Expedient Intern", "Expedient Extern", "Import Atorgat", "Pagament", "Import Total Actuació", "Obligacions", "Persona De Contacte", "Terminis", "Justificació", "Partida", "Justificació Verificada", "Imports A Retornar"
                };
                var columnesFitxer = dataTable.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName.Trim()).ToArray();
                var mapColFitxer = new Dictionary<string, int>();
                for (int i = 0; i < columnesFitxer.Length; i++)
                    mapColFitxer[RemoveDiacritics(columnesFitxer[i]).ToLowerInvariant()] = i;

                var missingColumns = new List<string>();
                var missingOptionalColumns = new List<string>();
                string[] columnesOpcionals = new string[] {
                    "Any", "Entitat", "Estat", "Ens", "Persona De Contacte", "Imports A Retornar"
                };
                foreach (var esperada in columnesEsperades)
                {
                    var key = RemoveDiacritics(esperada).ToLowerInvariant();
                    if (!mapColFitxer.ContainsKey(key))
                    {
                        if (columnesOpcionals.Contains(esperada))
                            missingOptionalColumns.Add(esperada);
                        else
                            missingColumns.Add(esperada);
                    }
                }
                if (missingColumns.Count > 0)
                {
                    resultat.Errors.Add("Falten columnes al fitxer:");
                    resultat.Errors.AddRange(missingColumns);
                    return resultat;
                }
                if (missingOptionalColumns.Count > 0)
                {
                    resultat.Advertencies.Add("Columnes opcionals no trobades:");
                    resultat.Advertencies.AddRange(missingOptionalColumns);
                }

                // Només afegeix a idxMap les columnes que existeixen al fitxer
                var idxMap = columnesEsperades
                    .Select(c => mapColFitxer.TryGetValue(RemoveDiacritics(c).ToLowerInvariant(), out var idx) ? idx : (int?)null)
                    .ToArray();

                // Processar el DataTable amb idxMap (passa idxMap amb nulls per columnes opcionals no trobades)
                await ProcessarDataTable(dataTable, anySeleccionatId, resultat, idxMap, mapColFitxer);

                if (!resultat.TéErrors)
                {
                    await _dbLogger.LogInformationAsync(
                        $"Importació completada: {resultat.Importats} noves, {resultat.Actualitzats} actualitzades, {resultat.SenseCanvis} sense canvis", 
                        "ImportarCSV", 
                        $"Total={resultat.Total}");
                }

                return resultat;
            }
            catch (Exception ex)
            {
                resultat.Errors.Add($"Error processant fitxer: {ex.Message}");
                await _dbLogger.LogFatalAsync($"Error crític processant fitxer d'importació: {rutaFitxer}", ex, "ImportarCSV", $"RutaFitxer={rutaFitxer}");
            }

            return resultat;
        }

        private async Task<DataTable> ConvertirExcelADataTable(string rutaFitxer, ResultatImportacio resultat)
        {
            var dt = new DataTable();

            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(rutaFitxer, false))
                {
                    WorkbookPart? workbookPart = doc.WorkbookPart;
                    if (workbookPart == null)
                    {
                        resultat.Errors.Add("No s'ha pogut obrir el WorkbookPart");
                        return dt;
                    }
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    Worksheet worksheet = worksheetPart.Worksheet;

                    // Obtenir les cel·les fusionades
                    MergeCells? mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
                    var mergedRanges = (mergeCells?.Elements<MergeCell>().Select(m => m.Reference?.Value ?? "").ToList() ?? new List<string>()).AsReadOnly();

                    SheetData sheetData = worksheet.Elements<SheetData>().First();
                    var rows = sheetData.Elements<Row>().ToList();

                    if (rows.Count == 0)
                    {
                        resultat.Errors.Add("Full d'Excel buit");
                        return dt;
                    }

                    // Header
                    var headerRow = rows[0];
                    var cells = headerRow.Elements<Cell>().ToList();
                    foreach (var cell in cells)
                    {
                        var columnName = GetCellValue(workbookPart, cell);
                        dt.Columns.Add(string.IsNullOrWhiteSpace(columnName) ? $"Column{dt.Columns.Count + 1}" : columnName);
                    }
                    resultat.ColumnesCapcalera = dt.Columns.Count;
                    resultat.DelimitadorDetectat = "Excel";

                    // Processar files de dades, agrupant fusionades
                    List<DataRow> logicalRows = new List<DataRow>();
                    DataRow? currentLogicalRow = null;

                    for (int i = 1; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        cells = row.Elements<Cell>().ToList();
                        if (cells.Count == 0) continue;

                        string regidor = cells.Count > 0 ? GetCellValue(workbookPart, cells[0]) : "";
                        bool isMerged = mergedRanges.Any(range => IsCellInRange(row.RowIndex!.Value, 1, range));

                        if (!string.IsNullOrEmpty(regidor) && !isMerged)
                        {
                            // Nova fila lògica
                            if (currentLogicalRow != null)
                                logicalRows.Add(currentLogicalRow);

                            currentLogicalRow = dt.NewRow();
                            for (int col = 0; col < dt.Columns.Count; col++)
                            {
                                if (col < cells.Count)
                                    currentLogicalRow[col] = GetCellValue(workbookPart, cells[col]);
                            }
                        }
                        else
                        {
                            // Continuació, afegir a la fila actual
                            if (currentLogicalRow != null)
                            {
                                for (int col = 0; col < dt.Columns.Count; col++)
                                {
                                    if (col < cells.Count)
                                    {
                                        string val = GetCellValue(workbookPart, cells[col]);
                                        if (!string.IsNullOrEmpty(val))
                                            currentLogicalRow[col] = (currentLogicalRow[col]?.ToString() ?? "") + " " + val;
                                    }
                                }
                            }
                        }
                    }

                    if (currentLogicalRow != null)
                        logicalRows.Add(currentLogicalRow);

                    foreach (var lr in logicalRows)
                    {
                        bool filaBuida = true;
                        foreach (var item in lr.ItemArray)
                        {
                            if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
                            {
                                filaBuida = false;
                                break;
                            }
                        }
                        if (!filaBuida)
                            dt.Rows.Add(lr);
                    }
                }
            }
            catch (Exception ex)
            {
                resultat.Errors.Add($"Error llegint fitxer Excel: {ex.Message}");
                await _dbLogger.LogFatalAsync($"Error crític llegint fitxer Excel: {rutaFitxer}", ex, "ConvertirExcelADataTable", $"RutaFitxer={rutaFitxer}");
            }

            return dt;
        }

        private async Task<DataTable> ConvertirCSVADataTable(string rutaFitxer, ResultatImportacio resultat)
        {
            var dt = new DataTable();

            try
            {
                using var reader = new StreamReader(rutaFitxer);
                
                // Llegir capçalera
                var headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    resultat.Errors.Add("Fitxer CSV buit");
                    return dt;
                }

                // Detectar delimitador
                char delimitador = DetectarDelimitador(headerLine);
                resultat.DelimitadorDetectat = delimitador.ToString();
                
                var headers = ParseCSVLine(headerLine, delimitador);
                resultat.ColumnesCapcalera = headers.Length;

                // Crear columnes
                foreach (var header in headers)
                {
                    dt.Columns.Add(string.IsNullOrWhiteSpace(header) ? $"Column{dt.Columns.Count + 1}" : header);
                }

                // Llegir files de dades
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = ParseCSVLine(line, delimitador);
                    var dataRow = dt.NewRow();
                    
                    for (int i = 0; i < Math.Min(values.Length, dt.Columns.Count); i++)
                    {
                        dataRow[i] = values[i];
                    }
                    
                    dt.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                resultat.Errors.Add($"Error llegint fitxer CSV: {ex.Message}");
                await _dbLogger.LogFatalAsync($"Error crític llegint fitxer CSV: {rutaFitxer}", ex, "ConvertirCSVADataTable", $"RutaFitxer={rutaFitxer}");
            }

            return dt;
        }

        private async Task ProcessarDataTable(DataTable dataTable, int? anySeleccionatId, ResultatImportacio resultat, int?[] idxMap, Dictionary<string, int> mapColFitxer)
        {
            // Verificar que tenim 17 columnes
            if (dataTable.Columns.Count < 17)
            {
                resultat.Errors.Add($"El fitxer ha de tenir 17 columnes (trobades {dataTable.Columns.Count})");
                return;
            }

            // Carregar diccionaris de mapejament amb un context separat
            using var contextForDictionaries = _contextFactory.CreateDbContext();
            var regidors = await contextForDictionaries.Regidors.ToDictionaryAsync(r => Normalitza(r.Nom), r => r.Id);
            // Diccionari per nom curt (primer token) només si és únic (normalitzat)
            var regidorsFirstName = (await contextForDictionaries.Regidors
                .Select(r => new { r.Id, Nom = Normalitza(r.Nom) })
                .ToListAsync())
                .GroupBy(r => r.Nom.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0])
                .Where(g => g.Count() == 1)
                .ToDictionary(g => g.Key, g => g.First().Id);
            var usuaris = await contextForDictionaries.Usuaris.Select(u => new { u.Id, Nom = Normalitza(u.Nom) }).ToDictionaryAsync(u => u.Nom, u => u.Id);
            var arees = await contextForDictionaries.Arees.ToListAsync();
            var entitats = await contextForDictionaries.Entitats.ToDictionaryAsync(e => Normalitza(e.Nom), e => e.Id);
            var estats = await contextForDictionaries.Estats.ToDictionaryAsync(e => Normalitza(e.Nom), e => e.Id);
            var ensList = await contextForDictionaries.Ens.ToDictionaryAsync(e => Normalitza(e.Nom), e => e.Id);

            // Processar cada fila
            int numeroLinia = 1; // La línia 1 és la capçalera
            foreach (DataRow row in dataTable.Rows)
            {
                numeroLinia++;
                try
                {
                    // Convertir DataRow a array de strings segons el mapping
                    string[] camps = new string[17];
                    for (int i = 0; i < 17; i++)
                    {
                        if (idxMap[i].HasValue)
                            camps[i] = row[idxMap[i]!.Value]?.ToString()?.Trim() ?? string.Empty;
                        else
                            camps[i] = string.Empty; // Si la columna és opcional i no existeix
                    }

                    // NO importar si la columna Subvencio (camps[3]) està buida o només té espais
                    if (string.IsNullOrWhiteSpace(camps[3]))
                        continue;

                    // Crear un nou context per cada fila
                    using var contextForRow = _contextFactory.CreateDbContext();
                    await ProcessarFilaSubvencio(camps, numeroLinia, anySeleccionatId, regidors, regidorsFirstName, usuaris, arees, entitats, estats, ensList, mapColFitxer, contextForRow, resultat);
                    await contextForRow.SaveChangesAsync(); // Guardar canvis per cada fila
                }
                catch (Exception ex)
                {
                    resultat.Errors.Add($"Línia {numeroLinia}: {ex.Message}");
                    await _dbLogger.LogFatalAsync($"Error crític processant línia {numeroLinia} de fitxer d'importació", ex, "ProcessarDataTable", $"NumeroLinia={numeroLinia}");
                }
            }

            // await _context.SaveChangesAsync(); // Ja no necessari, es fa per fila
        }

        private async Task ProcessarFilaSubvencio(
            string[] camps,
            int numeroLinia,
            int? anySeleccionatId,
            Dictionary<string, int> regidors,
            Dictionary<string, int> regidorsFirstName,
            Dictionary<string, int> usuaris,
            List<AreaModel> arees,
            Dictionary<string, int> entitats,
            Dictionary<string, int> estats,
            Dictionary<string, int> ensList,
            Dictionary<string, int> mapColFitxer,
            GestorSubvencionsContext context,
            ResultatImportacio resultat)
        {
            // ===== MAPEJAMENT DE COLUMNES =====
            // 0: REGIDOR, 1: ÀREA, 2: ORGANISME, 3: SUBVENCIÓ, 4: ACTUACIÓ, 5: EXPEDIENT INTERN,
            // 6: EXPEDIENT EXTERN, 7: IMPORT ATORGAT, 8: PAGAMENT, 9: IMPORT TOTAL ACTUACIÓ,
            // 10: OBLIGACIONS, 11: PERSONA DE CONTACTE, 12: TERMINIS, 13: JUSTIFICACIÓ,
            // 14: PARTIDA, 15: JUSTIFICACIÓ VERIFICADA, 16: IMPORTS A RETORNAR

            // ===== ENS: BASAT EN ORGANISME =====
            int? ensId = null;
            var ensNom = Normalitza(camps[2]); // Organisme
            if (!string.IsNullOrEmpty(ensNom))
            {
                if (ensList.TryGetValue(ensNom, out var id))
                {
                    ensId = id;
                }
                else
                {
                    // Crear nou ens
                    var nouEns = new EnsModel { Nom = camps[2], CodiEns = ensNom };
                    context.Ens.Add(nouEns);
                    await context.SaveChangesAsync();
                    ensList[ensNom] = nouEns.Id;
                    ensId = nouEns.Id;
                    resultat.MestresCreats++;
                }
            }

            // ===== OBTENIR/CREAR ÀREA =====
            int? areaId = null;
            var areaNom = Normalitza(camps[1]);
            if (!string.IsNullOrEmpty(areaNom))
            {
                var area = arees.FirstOrDefault(a => 
                    Normalitza(a.Nom) == areaNom && 
                    a.EnsId == ensId);
                
                if (area != null)
                {
                    areaId = area.Id;
                }
                else
                {
                    // Crear nova àrea
                    var novaArea = new AreaModel 
                    { 
                        Nom = camps[1], 
                        EnsId = ensId 
                    };
                    context.Arees.Add(novaArea);
                    await context.SaveChangesAsync();
                    arees.Add(novaArea);
                    areaId = novaArea.Id;
                    resultat.MestresCreats++;
                }
            }

            // ===== ENTITAT: NOU AL FITXER, SET TO NULL =====
            int? entitatId = null;

            // ===== ESTAT: BASAT EN EXPEDIENT EXTERN =====
            int? estatId = null;
            // Si hi ha columna Estat opcional, utilitzar-la
            if (mapColFitxer.TryGetValue("estat", out var estatColIdx))
            {
                var estatNom = Normalitza(camps[estatColIdx]);
                if (!string.IsNullOrEmpty(estatNom) && estats.TryGetValue(estatNom, out var idEstat))
                {
                    estatId = idEstat;
                }
            }

            // ===== REGIDOR I FONT RESPONSABLE =====
            int? regidorId = null;
            string? fontResponsable = null;
            var regidorNomOriginal = camps[0];
            var regidorNomNormalitzat = Normalitza(regidorNomOriginal);
            if (!string.IsNullOrEmpty(regidorNomNormalitzat))
            {
                // 1. Coincidència exacta del nom complet a Regidors
                if (regidors.TryGetValue(regidorNomNormalitzat, out var idExacte))
                {
                    regidorId = idExacte;
                    fontResponsable = "R";
                }
                else
                {
                    // 2. Coincidència pel primer token (nom propi) si és únic a Regidors
                    var primerToken = regidorNomNormalitzat.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                    if (regidorsFirstName.TryGetValue(primerToken, out var idPrimer))
                    {
                        regidorId = idPrimer;
                        fontResponsable = "R";
                    }
                    else
                    {
                        // 3. Coincidència parcial començant pel valor proporcionat (prefix) a Regidors
                        var candidates = regidors
                            .Where(kvp => kvp.Key.StartsWith(regidorNomNormalitzat + " "))
                            .Select(kvp => kvp.Value)
                            .Distinct()
                            .ToList();
                        if (candidates.Count == 1)
                        {
                            regidorId = candidates[0];
                            fontResponsable = "R";
                        }
                        else
                        {
                            // 4. Cercar a Usuaris
                            if (usuaris.TryGetValue(regidorNomNormalitzat, out var idUsuari))
                            {
                                // Crear nou regidor a partir de l'usuari
                                var nouRegidor = new RegidorModel { Nom = regidorNomOriginal };
                                context.Regidors.Add(nouRegidor);
                                await context.SaveChangesAsync();
                                regidorId = nouRegidor.Id;
                                fontResponsable = "U";
                                // Actualitzar diccionaris
                                regidors[regidorNomNormalitzat] = nouRegidor.Id;
                                regidorsFirstName[primerToken] = nouRegidor.Id; // Si és únic, però per simplificar
                            }
                            else
                            {
                                resultat.Advertencies.Add($"Línia {numeroLinia}: Regidor '{regidorNomOriginal}' no coincideix (exacte/prefix) amb cap registre a Regidors o Usuaris → NULL");
                            }
                        }
                    }
                }
            }
            // si està buit no afegim advertència

            // ===== PARSEJAR IMPORTS =====
            var importAtorgat = ParseImport(camps[7]);
            var importTotalActuacio = ParseImport(camps[9]);
            var importsRetornar = ParseImport(camps[16]);

            // ===== VERIFICAR SI JA EXISTEIX (dins l'any seleccionat) =====
            var subvencioExistent = await context.Subvencions
                .SingleOrDefaultAsync(s => 
                    s.Subvencio == camps[3] && 
                    s.AreaId == areaId && 
                    s.AnyId == anySeleccionatId);

            if (subvencioExistent != null)
            {
                // ===== ACTUALITZAR SI HI HA CANVIS =====
                bool hiHaCanvis = false;

                if (subvencioExistent.Actuacio != camps[4]) { subvencioExistent.Actuacio = camps[4]; hiHaCanvis = true; }
                if (subvencioExistent.AnyId != anySeleccionatId) { subvencioExistent.AnyId = anySeleccionatId; hiHaCanvis = true; }
                // ExpedientIntern i ExpedientExtern eliminats - ara es gestionen a ActualitzarExpedientsSubvencio
                if (subvencioExistent.ImportAtorgat != importAtorgat) { subvencioExistent.ImportAtorgat = importAtorgat; hiHaCanvis = true; }
                // Pagament eliminat - ara es gestiona a ActualitzarPagamentsSubvencio
                if (subvencioExistent.ImportTotalActuacio != importTotalActuacio) { subvencioExistent.ImportTotalActuacio = importTotalActuacio; hiHaCanvis = true; }
                if (subvencioExistent.Obligacions != camps[10]) { subvencioExistent.Obligacions = camps[10]; hiHaCanvis = true; }
                if (subvencioExistent.PersonaContacte != camps[11]) { subvencioExistent.PersonaContacte = camps[11]; hiHaCanvis = true; }
                if (subvencioExistent.Justificacio != camps[13]) { subvencioExistent.Justificacio = camps[13]; hiHaCanvis = true; }
                if (subvencioExistent.Partida != camps[14]) { subvencioExistent.Partida = camps[14]; hiHaCanvis = true; }
                if (subvencioExistent.JustificacioVerificada != camps[15]) { subvencioExistent.JustificacioVerificada = camps[15]; hiHaCanvis = true; }
                if (subvencioExistent.ImportsRetornar != importsRetornar) { subvencioExistent.ImportsRetornar = importsRetornar; hiHaCanvis = true; }
                if (subvencioExistent.FontResponsable != fontResponsable) { subvencioExistent.FontResponsable = fontResponsable; hiHaCanvis = true; }
                if (subvencioExistent.EntitatId != entitatId) { subvencioExistent.EntitatId = entitatId; hiHaCanvis = true; }
                if (subvencioExistent.EstatId != estatId) { subvencioExistent.EstatId = estatId; hiHaCanvis = true; }
                if (subvencioExistent.EnsId != ensId) { subvencioExistent.EnsId = ensId; hiHaCanvis = true; }

                // Actualitzar regidors responsables (many-to-many)
                // Elimina relacions antigues i afegeix la nova si no existeix
                var regidorSubvencionsExistents = await context.RegidorsSubvencions.Where(rs => rs.SubvencioId == subvencioExistent.Id).ToListAsync();
                context.RegidorsSubvencions.RemoveRange(regidorSubvencionsExistents);
                if (regidorId != null)
                {
                    var novaRelacio = new RegidorsSubvencions { SubvencioId = subvencioExistent.Id, RegidorId = regidorId.Value };
                    context.RegidorsSubvencions.Add(novaRelacio);
                }

                if (hiHaCanvis)
                {
                    resultat.Actualitzats++;
                }
                else
                {
                    resultat.SenseCanvis++;
                }

                // Si hi ha canvis o sempre que existeix, actualitzar terminis i expedients
                var terminisParsejats = ParsejarTermini(camps[12]);
                await ActualitzarTerminisSubvencio(subvencioExistent.Id, terminisParsejats, context);
                
                // Actualitzar expedients (ExpedientIntern i ExpedientExtern combinats)
                var expedientsCombinats = new List<string>();
                expedientsCombinats.AddRange(ParsejarExpedients(camps[5])); // ExpedientIntern
                expedientsCombinats.AddRange(ParsejarExpedients(camps[6])); // ExpedientExtern
                await ActualitzarExpedientsSubvencio(subvencioExistent.Id, expedientsCombinats, context);

                // Actualitzar pagaments
                var pagamentsParsejats = ParsejarPagaments(camps[8]);
                await ActualitzarPagamentsSubvencio(subvencioExistent.Id, pagamentsParsejats, subvencioExistent.ImportAtorgat, context);
            }
            else
            {
                // ===== CREAR NOU REGISTRE =====
                var novaSubvencio = new Subvencio_
                {
                    Subvencio = camps[3],
                    Actuacio = camps[4],
                    AnyId = anySeleccionatId,
                    AreaId = areaId,
                    EntitatId = entitatId,
                    EstatId = estatId,
                    EnsId = ensId,
                    // ExpedientIntern i ExpedientExtern eliminats - ara es gestionen a ActualitzarExpedientsSubvencio
                    ImportAtorgat = importAtorgat,
                    // Pagament eliminat - ara es gestiona a ActualitzarPagamentsSubvencio
                    ImportTotalActuacio = importTotalActuacio,
                    Obligacions = camps[10],
                    PersonaContacte = camps[11],
                    Justificacio = camps[13],
                    Partida = camps[14],
                    JustificacioVerificada = camps[15],
                    ImportsRetornar = importsRetornar,
                    FontResponsable = fontResponsable
                };

                context.Subvencions.Add(novaSubvencio);
                await context.SaveChangesAsync(); // Guardar per obtenir l'ID

                // Afegir relació many-to-many amb regidor
                if (regidorId != null)
                {
                    var novaRelacio = new RegidorsSubvencions { SubvencioId = novaSubvencio.Id, RegidorId = regidorId.Value };
                    context.RegidorsSubvencions.Add(novaRelacio);
                }

                // Processar terminis per a la nova subvenció
                var terminisParsejats = ParsejarTermini(camps[12]);
                await ActualitzarTerminisSubvencio(novaSubvencio.Id, terminisParsejats, context);
                
                // Processar expedients per a la nova subvenció (ExpedientIntern i ExpedientExtern combinats)
                var expedientsCombinats = new List<string>();
                expedientsCombinats.AddRange(ParsejarExpedients(camps[5])); // ExpedientIntern
                expedientsCombinats.AddRange(ParsejarExpedients(camps[6])); // ExpedientExtern
                await ActualitzarExpedientsSubvencio(novaSubvencio.Id, expedientsCombinats, context);

                // Processar pagaments per a la nova subvenció
                var pagamentsParsejats = ParsejarPagaments(camps[8]);
                await ActualitzarPagamentsSubvencio(novaSubvencio.Id, pagamentsParsejats, novaSubvencio.ImportAtorgat, context);

                resultat.Importats++;
            }
        }

        private string[] ParseCSVLine(string line, char delimitador)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    // Mirar si és un escape d'una cometa dins d'un camp citat
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // saltar la segona cometa
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimitador && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        private char DetectarDelimitador(string headerLine)
        {
            char[] candidats = new[] { ';', ',', '\t' };
            int millorCount = -1;
            char millor = ';';
            foreach (var c in candidats)
            {
                var parts = ParseCSVLine(headerLine, c);
                if (parts.Length > millorCount)
                {
                    millorCount = parts.Length;
                    millor = c;
                }
            }
            return millor;
        }

        private decimal? ParseImport(string importText)
        {
            if (string.IsNullOrWhiteSpace(importText))
                return null;

            importText = importText.Replace(".", "").Replace(",", ".");
            
            if (decimal.TryParse(importText, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            
            return null;
        }

        private static string Normalitza(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var upper = text.ToUpperInvariant().Trim();
            var formD = upper.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }
            // Normalitzar espais múltiples
            var cleaned = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ");
            return cleaned.Normalize(NormalizationForm.FormC);
        }

        // Funció auxiliar per obtenir el valor d'una cel·la
        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell == null) return "";
            string value = cell.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart? stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable != null)
                    value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }
            return value;
        }

        // Funció auxiliar per comprovar si una cel·la està en un rang fusionat
        private bool IsCellInRange(uint rowIndex, uint colIndex, string range)
        {
            var parts = range.Split(':');
            if (parts.Length != 2) return false;
            var start = GetRowCol(parts[0]);
            var end = GetRowCol(parts[1]);
            return rowIndex >= start.row && rowIndex <= end.row && colIndex >= start.col && colIndex <= end.col;
        }

        private (uint row, uint col) GetRowCol(string cellRef)
        {
            var match = System.Text.RegularExpressions.Regex.Match(cellRef, @"([A-Z]+)(\d+)");
            if (!match.Success) return (0, 0);
            string colStr = match.Groups[1].Value;
            uint col = 0;
            for (int i = 0; i < colStr.Length; i++)
                col = col * 26 + (uint)(colStr[i] - 'A' + 1);
            uint row = uint.Parse(match.Groups[2].Value);
            return (row, col);
        }

        // Mètode per parsejar el text dels terminis
        private List<TerminiInfo> ParsejarTermini(string terminiText)
        {
            var terminis = new List<TerminiInfo>();

            if (string.IsNullOrWhiteSpace(terminiText))
                return terminis;

            // Dividir per salts de línia o punts i coma
            var parts = terminiText.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .Where(p => !string.IsNullOrWhiteSpace(p))
                                   .ToArray();

            foreach (var part in parts)
            {
                var terminiInfo = new TerminiInfo();

                // Patró típic: "Execució: 01/01/2024 - 31/12/2024"
                var colonIndex = part.IndexOf(':');
                if (colonIndex > 0)
                {
                    terminiInfo.Nom = part.Substring(0, colonIndex).Trim();
                    var datesPart = part.Substring(colonIndex + 1).Trim();

                    // Cercar dates en format dd/mm/yyyy
                    var dateRegex = new System.Text.RegularExpressions.Regex(@"(\d{1,2})/(\d{1,2})/(\d{4})");
                    var matches = dateRegex.Matches(datesPart);

                    if (matches.Count >= 1)
                    {
                        try
                        {
                            terminiInfo.DataInici = ParseDate(matches[0].Value);
                        }
                        catch { /* Ignorar errors de parsing */ }
                    }

                    if (matches.Count >= 2)
                    {
                        try
                        {
                            terminiInfo.DataFi = ParseDate(matches[1].Value);
                        }
                        catch { /* Ignorar errors de parsing */ }
                    }

                    // La resta podria ser notes
                    var datesText = string.Join(" - ", matches.Select(m => m.Value));
                    var notesStart = datesPart.IndexOf(datesText);
                    if (notesStart >= 0)
                    {
                        var notesPart = datesPart.Substring(notesStart + datesText.Length).Trim();
                        if (!string.IsNullOrWhiteSpace(notesPart) && notesPart.Length > 1)
                        {
                            terminiInfo.Notes = notesPart.TrimStart('-', ' ').Trim();
                        }
                    }
                }
                else
                {
                    // Si no hi ha ':', tractar tot com a nom
                    terminiInfo.Nom = part;
                }

                if (!string.IsNullOrWhiteSpace(terminiInfo.Nom))
                {
                    terminis.Add(terminiInfo);
                }
            }

            return terminis;
        }

        // Mètode auxiliar per parsejar dates
        private DateTime? ParseDate(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;
            if (DateTime.TryParseExact(dateStr, "d/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;
            return null;
        }

        // Mètode per parsejar expedients separats per comes, punts i coma o espais
        private List<string> ParsejarExpedients(string? textExpedients)
        {
            var expedients = new List<string>();
            
            if (string.IsNullOrWhiteSpace(textExpedients))
                return expedients;

            // Dividir per comes, punts i coma o espais
            var parts = textExpedients.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var expedient = part.Trim();
                if (!string.IsNullOrWhiteSpace(expedient) && expedient.Length <= 20)
                {
                    expedients.Add(expedient);
                }
            }

            return expedients;
        }

        // Mètode per actualitzar els expedients d'una subvenció
        private async Task ActualitzarExpedientsSubvencio(int subvencioId, List<string> expedients, GestorSubvencionsContext context)
        {
            // Obtenir expedients existents per aquesta subvenció
            var expedientsExistents = await context.Expedients
                .Where(e => e.SubvencioId == subvencioId)
                .ToListAsync();

            // Eliminar expedients existents
            context.Expedients.RemoveRange(expedientsExistents);

            // Eliminar duplicats de la llista d'expedients
            var expedientsUnics = expedients.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Afegir nous expedients
            foreach (var numeroExpedient in expedientsUnics)
            {
                var expedient = new Expedient
                {
                    SubvencioId = subvencioId,
                    NumeroExpedient = numeroExpedient
                };
                context.Expedients.Add(expedient);
            }
        }

        // Mètode per actualitzar els terminis d'una subvenció
        private async Task ActualitzarTerminisSubvencio(int subvencioId, List<TerminiInfo> terminis, GestorSubvencionsContext context)
        {
            // Obtenir terminis existents per aquesta subvenció
            var terminisExistents = await context.TerminisSubvencions
                .Where(ts => ts.SubvencioId == subvencioId)
                .ToListAsync();

            // Eliminar terminis existents
            context.TerminisSubvencions.RemoveRange(terminisExistents);

            // Afegir nous terminis
            foreach (var terminiInfo in terminis)
            {
                // Obtenir o crear el termini
                var termini = await context.Terminis
                    .FirstOrDefaultAsync(t => t.Nom == terminiInfo.Nom);

                if (termini == null)
                {
                    termini = new Termini { Nom = terminiInfo.Nom };
                    context.Terminis.Add(termini);
                    await context.SaveChangesAsync(); // Necessari per obtenir l'ID
                }

                // Crear la relació
                var terminiSubvencio = new TerminisSubvencio
                {
                    SubvencioId = subvencioId,
                    TerminiId = termini.Id,
                    DataInici = terminiInfo.DataInici,
                    DataFi = terminiInfo.DataFi,
                    Notes = terminiInfo.Notes
                };

                context.TerminisSubvencions.Add(terminiSubvencio);
            }
        }

        // Mètode per parsejar pagaments amb formats complexos
        private List<PagamentInfo> ParsejarPagaments(string pagamentText)
        {
            var pagaments = new List<PagamentInfo>();

            if (string.IsNullOrWhiteSpace(pagamentText))
                return pagaments;

            // Dividir per salts de línia o punts i coma
            var parts = pagamentText.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .Where(p => !string.IsNullOrWhiteSpace(p))
                                   .ToArray();

            foreach (var part in parts)
            {
                var pagamentInfo = new PagamentInfo();

                // Cas especial: "x" indica sense pagament
                if (part.Trim().ToLower() == "x")
                {
                    pagamentInfo.IsNoPayment = true;
                    pagaments.Add(pagamentInfo);
                    continue;
                }

                // Cercar imports amb símbols d'euro
                var euroRegex = new System.Text.RegularExpressions.Regex(@"(\d+(?:[.,]\d+)*)\s*€");
                var euroMatch = euroRegex.Match(part);
                if (euroMatch.Success)
                {
                    var amountText = euroMatch.Groups[1].Value.Replace(",", ".");
                    if (decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    {
                        pagamentInfo.Amount = amount;
                        pagamentInfo.Currency = "EUR";
                    }

                    // La resta del text després de l'import
                    var remainingText = part.Substring(euroMatch.Index + euroMatch.Length).Trim();

                    // Cercar dates en format dd/mm/yyyy o dd/mm/yy
                    var dateRegex = new System.Text.RegularExpressions.Regex(@"(\d{1,2})/(\d{1,2})/(\d{2,4})");
                    var dateMatch = dateRegex.Match(remainingText);
                    if (dateMatch.Success)
                    {
                        try
                        {
                            pagamentInfo.Date = ParseDate(dateMatch.Value);
                            // El text restant després de la data és el comentari
                            var commentStart = remainingText.IndexOf(dateMatch.Value) + dateMatch.Length;
                            if (commentStart < remainingText.Length)
                            {
                                pagamentInfo.Comment = remainingText.Substring(commentStart).Trim();
                            }
                        }
                        catch { /* Ignorar errors de parsing */ }
                    }
                    else
                    {
                        // Si no hi ha data, tot el restant és comentari
                        pagamentInfo.Comment = remainingText;
                    }
                }
                else
                {
                    // Si no hi ha import, tractar com a descripció de servei o comentari
                    // Cercar dates per identificar dates de concessió
                    var dateRegex = new System.Text.RegularExpressions.Regex(@"(\d{1,2})/(\d{1,2})/(\d{2,4})");
                    var dateMatch = dateRegex.Match(part);
                    if (dateMatch.Success)
                    {
                        try
                        {
                            pagamentInfo.Date = ParseDate(dateMatch.Value);
                            pagamentInfo.IsGrantDate = true;
                            // El text abans de la data podria ser descripció
                            var descPart = part.Substring(0, dateMatch.Index).Trim();
                            if (!string.IsNullOrWhiteSpace(descPart))
                            {
                                pagamentInfo.ServiceDescription = descPart;
                            }
                            // El text després de la data és comentari
                            var commentStart = dateMatch.Index + dateMatch.Length;
                            if (commentStart < part.Length)
                            {
                                pagamentInfo.Comment = part.Substring(commentStart).Trim();
                            }
                        }
                        catch { /* Ignorar errors de parsing */ }
                    }
                    else
                    {
                        // Sense import ni data, tractar com a descripció de servei
                        pagamentInfo.ServiceDescription = part;
                    }
                }

                // Calcular percentatge de pagament si hi ha import
                if (pagamentInfo.Amount.HasValue)
                {
                    // Això es calcularà més tard quan tinguem l'import total atorgat
                    // Per ara, deixem PaymentPercentage null
                }

                pagaments.Add(pagamentInfo);
            }

            return pagaments;
        }

        // Mètode per actualitzar els pagaments d'una subvenció
        private async Task ActualitzarPagamentsSubvencio(int subvencioId, List<PagamentInfo> pagaments, decimal? importAtorgat, GestorSubvencionsContext context)
        {
            // Obtenir pagaments existents per aquesta subvenció
            var pagamentsExistents = await context.Pagaments
                .Where(p => p.SubvencioId == subvencioId)
                .ToListAsync();

            // Eliminar pagaments existents
            context.Pagaments.RemoveRange(pagamentsExistents);

            // Afegir nous pagaments
            foreach (var pagamentInfo in pagaments)
            {
                // Calcular percentatge de pagament si tenim import atorgat
                int? paymentPercentage = null;
                if (pagamentInfo.Amount.HasValue && importAtorgat.HasValue && importAtorgat.Value > 0)
                {
                    paymentPercentage = (int?)Math.Round((pagamentInfo.Amount.Value / importAtorgat.Value) * 100);
                }

                var pagament = new Pagament
                {
                    SubvencioId = subvencioId,
                    Amount = pagamentInfo.Amount,
                    Currency = pagamentInfo.Currency,
                    Date = pagamentInfo.Date,
                    Comment = pagamentInfo.Comment,
                    PaymentPercentage = paymentPercentage,
                    IsNoPayment = pagamentInfo.IsNoPayment,
                    ServiceDescription = pagamentInfo.ServiceDescription,
                    IsGrantDate = pagamentInfo.IsGrantDate
                };

                context.Pagaments.Add(pagament);
            }
        }
    }
    public class ResultatImportacio
    {
        public int Importats { get; set; }
        public int Actualitzats { get; set; }
        public int SenseCanvis { get; set; }
        public int MestresCreats { get; set; }
        public List<string> Advertencies { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string? DelimitadorDetectat { get; set; }
        public int ColumnesCapcalera { get; set; }
        
        public int Total => Importats + Actualitzats + SenseCanvis;
        public bool TéErrors => Errors.Any();
        public bool TéAdvertencies => Advertencies.Any();
    }

    // Classe auxiliar per representar la informació parsejada d'un termini
    public class TerminiInfo
    {
        public string Nom { get; set; } = string.Empty;
        public DateTime? DataInici { get; set; }
        public DateTime? DataFi { get; set; }
        public string? Notes { get; set; }
    }

    // Classe auxiliar per representar la informació parsejada d'un pagament
    public class PagamentInfo
    {
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? Date { get; set; }
        public string? Comment { get; set; }
        public int? PaymentPercentage { get; set; }
        public bool IsNoPayment { get; set; }
        public string? ServiceDescription { get; set; }
        public bool IsGrantDate { get; set; }
    }

}
