# Sistema de Logging - GestorSubvencions

## Descripció
S'ha implementat un sistema de logging que emmagatzema els logs a la base de dades MySQL, amb filtrat automàtic segons l'entorn (Development/Production).

### Configuració per Entorn

#### **Producció** (appsettings.json)
```json
"DatabaseLogging": {
  "ProductionLevel": "Fatal"
}
```
- Només es guarden logs **FATAL** (errors crítics en catch blocks)
- Minimitza l'impacte en el rendiment
- Redueix el volum de dades emmagatzemades

#### **Development** (appsettings.Development.json)
```json
"DatabaseLogging": {
  "DevelopmentLevel": "Debug"
}
```
- Es guarden **tots** els nivells: Debug, Information, Warning, Error, Fatal
- Màxima visibilitat per a debugging
- Logs d'APIs, operacions importants, avisos

### Nivells de Log

| Nivell | Producció | Development | Ús |
|--------|-----------|-------------|-----|
| **Debug** | ❌ | ✅ | Crides a APIs, paràmetres, traces detallades |
| **Information** | ❌ | ✅ | Inici/fi operacions, resultats obtinguts, esdeveniments normals |
| **Warning** | ❌ | ✅ | Situacions inesperades però no crítiques (API retorna 0 resultats, placeholder detectat) |
| **Error** | ❌ | ✅ | Errors recuperables (retry exitós, errors menors) |
| **Fatal** | ✅ | ✅ | Errors crítics irrecuperables en catch blocks |

### Taules de Base de Dades

#### 1. **logs** - Logs d'aplicació
- Emmagatzema errors, warnings i informació de debug
- **Retenció**: 30 dies (purga automàtica diària a les 2:00 AM)
- **Nivells**: Debug, Information, Warning, Error, Fatal

#### 2. **audit_logs** - Logs d'auditoria
- Registra tots els canvis a les dades (INSERT, UPDATE, DELETE)
- **Retenció**: Indefinida (per compliment normatiu)

### Ús del DatabaseLoggerService

#### Injecció de Dependències
```csharp
public class MyController : ControllerBase
{
    private readonly DatabaseLoggerService _dbLogger;

    public MyController(DatabaseLoggerService dbLogger)
    {
        _dbLogger = dbLogger;
    }
}
```

#### Exemple de Logging en Try-Catch (PRODUCCIÓ)
```csharp
try
{
    // Codi que pot fallar
    await DoSomethingAsync();
}
catch (Exception ex)
{
    // Log FATAL per errors crítics (SEMPRE es guarda)
    await _dbLogger.LogFatalAsync(
        message: "Error crític durant l'operació X",
        exception: ex,
        action: "NomDelMètode",
        additionalData: "Context adicional si cal"
    );
    
    return StatusCode(500, "Error intern");
}
```

#### Exemple de Logging Complet (DEVELOPMENT)
```csharp
public async Task<ResultatOperacio> ProcessarDades(int id)
{
    // DEBUG: Traces detallades (només development)
    await _dbLogger.LogDebugAsync($"Processant dades per ID: {id}", "ProcessarDades");
    
    // INFO: Inici d'operació important (només development)
    await _dbLogger.LogInformationAsync("Iniciant processament de dades", "ProcessarDades", $"ID={id}");
    
    try
    {
        var dades = await _api.ObtenirDades(id);
        
        // WARN: Condicions inesperades però no crítiques (només development)
        if (dades == null || dades.Count == 0)
        {
            await _dbLogger.LogWarningAsync($"No s'han trobat dades per ID {id}", "ProcessarDades");
            return ResultatOperacio.Empty;
        }
        
        // INFO: Resultat exitós (només development)
        await _dbLogger.LogInformationAsync(
            $"Processament completat: {dades.Count} registres", 
            "ProcessarDades", 
            $"Total={dades.Count}");
        
        return ResultatOperacio.Success(dades);
    }
    catch (Exception ex)
    {
        // FATAL: Errors crítics (SEMPRE es guarda en producció i development)
        await _dbLogger.LogFatalAsync(
            $"Error crític processant dades per ID {id}", 
            ex, 
            "ProcessarDades", 
            $"ID={id}");
        
        return ResultatOperacio.Error;
    }
}
```

#### Altres Nivells de Log
```csharp
// Error (menys crític que Fatal, només development)
await _dbLogger.LogErrorAsync("Error procesant dades", ex, "ProcessData");

// Warning (advertències, només development)
await _dbLogger.LogWarningAsync("Dades incompletes", "ValidateData");

// Information (informació general, només development)
await _dbLogger.LogInformationAsync("Operació completada", "SaveData");

// Debug (detalls tècnics, només development)
await _dbLogger.LogDebugAsync("Processat registre #123", "ProcessRecord");
```

### Dades Capturades Automàticament
El servei captura automàticament:
- **Timestamp**: Data i hora UTC
- **Level**: Nivell del log (Debug, Information, Warning, Error, Fatal)
- **Message**: Missatge descriptiu de l'event
- **Exception**: Stack trace completa de l'excepció (si n'hi ha)
- **UserId**: Nom de l'usuari autenticat (si està disponible)
- **Action**: Nom del mètode o acció que genera el log
- **IpAddress**: Adreça IP del client
- **SessionId**: ID de sessió HTTP
- **AdditionalData**: Dades de context addicionals

### Filtrat Automàtic per Entorn

El **DatabaseLoggerService** automàticament filtra els logs segons l'entorn:

```csharp
// En PRODUCCIÓ (appsettings.json):
// ProductionLevel = "Fatal"
// → Només es guarden logs Fatal

// En DEVELOPMENT (appsettings.Development.json):
// DevelopmentLevel = "Debug"  
// → Es guarden tots els logs: Debug, Information, Warning, Error, Fatal
```

Això significa que pots cridar `LogDebugAsync` o `LogInformationAsync` al codi sense preocupar-te, ja que en producció no es guardaran a la BD.

### Exemples de Logs Implementats

#### ComarcaApiService
- **INFO**: Inici sincronització, resultats obtinguts, fi sincronització
- **DEBUG**: URL de l'API cridada
- **WARN**: API no respon, no hi ha dades
- **FATAL**: Errors crítics

#### ImportadorSubvencions  
- **INFO**: Inici importació, resultats (X noves, Y actualitzades)
- **DEBUG**: Fitxer i extensió processats
- **WARN**: Fitxer no trobat
- **FATAL**: Errors crítics en lectura o processament

#### APIs Externes (Ordenances, Establiments, Convenis, Electoral)
- **DEBUG**: URLs cridades amb paràmetres
- **INFO**: Resultats obtinguts (X elements)
- **WARN**: API retorna status incorrecte o 0 resultats
- **FATAL**: Errors crítics

#### AuthController
- **INFO**: Login/logout iniciats
- **FATAL**: Errors durant autenticació

#### ProxyController
- **DEBUG**: Sol·licituds d'imatges, placeholders detectats
- **WARN**: Errors obtenint imatges
- **FATAL**: Errors crítics

### Recomanacions per Desenvolupament

#### Quan usar cada nivell:

**Debug** 🔍
- Traces detallades de flux d'execució
- Paràmetres d'entrada/sortida
- URLs i configuracions de crides a APIs
```csharp
await _dbLogger.LogDebugAsync($"Cridant API: {url}", "GetData");
```

**Information** ℹ️
- Inici/fi d'operacions importants
- Resultats obtinguts (quants registres, etc.)
- Esdeveniments significatius del sistema
```csharp
await _dbLogger.LogInformationAsync($"S'han obtingut {count} registres", "GetData");
```

**Warning** ⚠️
- Situacions inesperades però no bloquejants
- API retorna 0 resultats quan s'esperaven dades
- Retry necessari
- Dades incompletes o amb format estrany
```csharp
await _dbLogger.LogWarningAsync("API no ha retornat dades", "GetData");
```

**Error** ❌ (rarament usat)
- Errors recuperables on l'aplicació pot continuar
- Millor usar FATAL en catch blocks
```csharp
await _dbLogger.LogErrorAsync("Error temporal", ex, "ProcessData");
```

**Fatal** 💀
- **SEMPRE** en catch blocks
- Errors que impedeixen completar l'operació
- Situacions crítiques que requereixen atenció immediata
```csharp
catch (Exception ex)
{
    await _dbLogger.LogFatalAsync("Error crític", ex, "ProcessData");
}
```

### Recomanacions per Producció

#### Què loguejar a producció:
- ✅ **Fatal**: Errors crítics que impedeixen l'operació (SEMPRE)
- ✅ **Error**: Errors recuperables però importants
- ⚠️ **Warning**: Advertències ocasionals
- ❌ **Information/Debug**: Evitar en producció (massa volum)

#### Configuració recomanada per entorns:
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Error",
      "Microsoft": "Warning"
    }
  }
}

// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

### Auditoria Automàtica

S'ha implementat un **override de `SaveChangesAsync`** al `GestorSubvencionsContext` que captura **AUTOMÀTICAMENT** tots els canvis a la base de dades:

#### Característiques:
- ✅ **Captura automàtica** de tots els INSERT, UPDATE, DELETE
- ✅ **Valors anteriors** (old_values) per a UPDATE i DELETE
- ✅ **Valors nous** (new_values) per a INSERT i UPDATE
- ✅ **Usuari autenticat** si està disponible
- ✅ **Timestamp UTC** del canvi
- ✅ **Taula i ID** del registre modificat
- ✅ **No afecta el rendiment** (operació asíncrona)

#### Com funciona:
```csharp
// NO cal fer res! L'auditoria és automàtica
await context.Voluntaris.AddAsync(nouVoluntari);
await context.SaveChangesAsync();  
// ↑ Automàticament guarda a audit_logs:
// - TableName: "Voluntari"
// - RecordId: "123"
// - Action: "Added"
// - NewValues: {"Nom":"Joan","Email":"joan@example.com",...}
// - UserId: "admin@example.com"
// - Timestamp: "2026-01-23T15:30:00Z"
```

#### Exemples d'auditoria:

**INSERT (nou voluntari)**:
```json
{
  "TableName": "Voluntari",
  "RecordId": "42",
  "Action": "Added",
  "NewValues": {
    "Id": 42,
    "Nom": "Maria",
    "Email": "maria@example.com",
    "Telefon": "600123456",
    "DataIncorporacio": "2026-01-23"
  },
  "UserId": "admin@santamariademartorelles.cat"
}
```

**UPDATE (modificar voluntari)**:
```json
{
  "TableName": "Voluntari",
  "RecordId": "42",
  "Action": "Modified",
  "OldValues": {
    "Telefon": "600123456",
    "Email": "maria@example.com"
  },
  "NewValues": {
    "Telefon": "611222333",
    "Email": "maria.nova@example.com"
  },
  "UserId": "admin@santamariademartorelles.cat"
}
```

**DELETE (eliminar voluntari)**:
```json
{
  "TableName": "Voluntari",
  "RecordId": "42",
  "Action": "Deleted",
  "OldValues": {
    "Id": 42,
    "Nom": "Maria",
    "Email": "maria.nova@example.com",
    "Telefon": "611222333"
  },
  "UserId": "admin@santamariademartorelles.cat"
}
```

#### Consultes útils per auditoria:

**Veure canvis d'un voluntari específic:**
```sql
SELECT * FROM audit_logs 
WHERE table_name = 'Voluntari' AND record_id = '42'
ORDER BY timestamp DESC;
```

**Veure què ha canviat un usuari avui:**
```sql
SELECT * FROM audit_logs 
WHERE user_id = 'admin@santamariademartorelles.cat'
  AND DATE(timestamp) = CURDATE()
ORDER BY timestamp DESC;
```

**Veure tots els canvis a voluntaris:**
```sql
SELECT 
    timestamp,
    action,
    record_id,
    user_id,
    old_values,
    new_values
FROM audit_logs 
WHERE table_name = 'Voluntari'
ORDER BY timestamp DESC
LIMIT 100;
```

**Resum de canvis per taula:**
```sql
SELECT 
    table_name,
    action,
    COUNT(*) as total,
    MAX(timestamp) as last_change
FROM audit_logs 
GROUP BY table_name, action
ORDER BY table_name, action;
```

### Consultes Útils per Logs d'Aplicació

#### Veure errors recents:
```sql
SELECT * FROM logs 
WHERE level IN ('Error', 'Fatal') 
ORDER BY timestamp DESC 
LIMIT 50;
```

#### Comptar errors per acció:
```sql
SELECT action, COUNT(*) as total, MAX(timestamp) as last_error
FROM logs 
WHERE level = 'Fatal' 
GROUP BY action 
ORDER BY total DESC;
```

#### Veure canvis d'auditoria per usuari:
```sql
SELECT * FROM audit_logs 
WHERE user_id = 'nom.usuari@example.com' 
ORDER BY timestamp DESC;
```

### Protecció de Dades (GDPR)
- Les IPs s'emmagatzemen per seguretat/debug però considera **anonimitzar-les** en producció
- Els logs es purgen automàticament cada 30 dies
- Els audit_logs es conserven indefinidament (requeriment normatiu)

### Troubleshooting

#### Si no es guarden logs:
1. Verifica que les taules existeixen: Executa [UpdateSQL_001.sql](../SQL/UpdateSQL_001.sql)
2. Comprova la connexió a MySQL
3. Revisa els permisos d'escriptura a les taules

#### Si falla la purga automàtica:
- El hosting pot no tenir `event_scheduler` activat
- Contacta el proveïdor o implementa la purga des de .NET (BackgroundService)

### Pròxims Passos
1. ✅ Crear taules `logs` i `audit_logs`
2. ✅ Implementar `DatabaseLoggerService`
3. ✅ Afegir logging als try-catch de controllers
4. ⏳ Afegir logging a serveis crítics
5. ⏳ Implementar auditoria automàtica amb SaveChanges override
6. ⏳ Configurar nivells de log per entorn (Production vs Development)

---
**Data d'implementació**: Gener 2026  
**Mantenidor**: Equip de Desenvolupament
