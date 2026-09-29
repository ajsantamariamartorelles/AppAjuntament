# Importació de Subvencions des d'Excel/CSV

## Descripció

Aquest mòdul permet importar subvencions massivament des d'un fitxer CSV/Excel a la base de dades.

## Ubicació

La funcionalitat d'importació està disponible a la pàgina de **Gestió de Subvencions** (`/subvencions`).

## Com utilitzar-lo

### 1. Preparar el fitxer CSV

El fitxer CSV ha de tenir les següents columnes **en aquest ordre exacte**:

| # | Columna | Tipus | Obligatori | Descripció |
|---|---------|-------|------------|------------|
| 1 | SUBVENCIÓ | Text | Sí | Nom de la subvenció |
| 2 | ACTUACIÓ | Text | No | Descripció de l'actuació |
| 3 | ANY | Número | No | Any de la subvenció (ex: 2024) |
| 4 | AREA | Text | No | Nom de l'àrea responsable |
| 5 | ENTITAT | Text | No | Nom de l'entitat beneficiària |
| 6 | ESTAT | Text | No | Estat actual de la subvenció |
| 7 | REGIDOR | Text | No | Nom del regidor responsable |
| 8 | ENS | Text | No | Nom de l'ens |
| 9 | IMPORT ATORGAT | Decimal | No | Import atorgat (format: 1.234,56) |
| 10 | EXPEDIENT INTERN | Text | No | Número d'expedient intern |
| 11 | EXPEDIENT EXTERN | Text | No | Número d'expedient extern |

#### Exemple de fitxer CSV:

```csv
SUBVENCIÓ;ACTUACIÓ;ANY;AREA;ENTITAT;ESTAT;REGIDOR;ENS;IMPORT ATORGAT;EXPEDIENT INTERN;EXPEDIENT EXTERN
"Subvenció esportiva 2024";"Activitats esportives";2024;"Esports";"Club Esportiu X";"Pendent";"Joan Garcia";"Ajuntament";5000,00;"EXP-2024-001";"EXT-001"
"Ajuda cultural";"Festival de música";2024;"Cultura";"Associació Cultural Y";"Aprovada";"Maria Lopez";"Ajuntament";3500,50;"EXP-2024-002";"EXT-002"
```

**Notes importants:**
- El separador ha de ser **punt i coma (;)**
- Els imports han de seguir el format català: `1.234,56` (punt per milers, coma per decimals)
- Les dates i textos amb caràcters especials han d'anar entre cometes dobles: `"text"`
- La primera línia ha de ser la capçalera amb els noms de les columnes

### 2. Importar el fitxer

1. Accedir a la pàgina de **Gestió de Subvencions**
2. Clicar el botó **"Importar Excel"** (verd, a la dreta del botó "Nova Subvenció")
3. Al diàleg que s'obre, clicar **"Selecciona el fitxer CSV/Excel"**
4. Seleccionar el fitxer CSV preparat
5. Revisar la informació mostrada
6. Clicar **"Importar"**
7. Esperar el processament (es mostra un spinner)
8. Revisar els resultats de la importació

## Comportament de la importació

### Gestió de referències externes

El sistema busca automàticament els **IDs** corresponents a les taules mestres:
- `ANY` → Taula `anys`
- `AREA` → Taula `arees`
- `ENTITAT` → Taula `entitats`
- `ESTAT` → Taula `estats`
- `REGIDOR` → Taula `regidors`
- `ENS` → Taula `ens`

**Si una referència NO existeix:**
- El camp es desa com a `NULL`
- S'afegeix una **advertència** al resum d'importació
- **La línia NO es descarta**, continua el processament

### Gestió de duplicats

El sistema considera que una subvenció és **duplicada** si ja existeix un registre amb:
- Mateix `Subvencio` (nom)
- Mateix `AnyId`
- Mateix `EntitatId`

**Si es detecta un duplicat:**
- Es comparen TOTES les propietats
- Si hi ha **canvis**, s'actualitza el registre existent
- Si **NO hi ha canvis**, el registre es compta com "sense canvis"

### Resultats de la importació

Després de la importació, es mostra un resum amb:

- ✅ **Registres importats**: Noves subvencions creades
- 🔄 **Registres actualitzats**: Subvencions existents que han estat modificades
- ⏸️ **Registres sense canvis**: Subvencions que ja existien i no han canviat
- ⚠️ **Advertències**: Referències externes no trobades (es guarden com a NULL)
- ❌ **Errors**: Problemes crítics que han impedit processar una línia

## Exemples de missatges

### Advertència
```
Línia 5: Àrea 'Departament Inexistent' no existeix → NULL
```
→ La subvenció s'ha importat però el camp `AreaId` és `NULL`

### Error
```
Línia 12: Object reference not set to an instance of an object
```
→ La línia 12 NO s'ha processat per un error crític

## Limitacions

- Mida màxima del fitxer: **10 MB**
- Formats acceptats: `.csv`, `.xlsx`, `.xls`
- Només es processen fitxers CSV amb separador `;`

## Solució de problemes

### "Error en processar la importació"
- Verificar que el fitxer té el format correcte
- Comprovar que el separador és punt i coma (;)
- Assegurar que la capçalera coincideix exactament

### "No s'han importat registres"
- Revisar la secció d'errors al resum
- Verificar que les referències externes existeixen a les taules mestres

### "Moltes advertències sobre referències no trobades"
- Abans d'importar subvencions, assegurar que existeixen:
  - Els anys a la taula `anys`
  - Les àrees a la taula `arees`
  - Les entitats a la taula `entitats`
  - Els estats a la taula `estats`
  - Els regidors a la taula `regidors`
  - Els ens a la taula `ens`

## Codi rellevant

- **Servei**: `Services/ImportadorSubvencions.cs`
- **Pàgina**: `Pages/Models/Subvencions/Subvencions.razor`
- **Registre**: `Program.cs` (línia amb `builder.Services.AddScoped<ImportadorSubvencions>()`)
