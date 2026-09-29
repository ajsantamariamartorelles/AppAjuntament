# AGENTS.md

Pautes per a qualsevol persona o eina d'IA que treballi en aquest repositori.
Només els responsables del repositori poden modificar aquest fitxer (vegeu `CODEOWNERS`).

## El projecte

Aplicació municipal de l'Ajuntament de Santa Maria de Martorelles (llicència EUPL-1.2):

- **API i web** (ASP.NET Core + Blazor Server, .NET 10): gestió de subvencions, patrimoni, armes, cursets, voluntariat i tercers.
- **App mòbil** (.NET MAUI): accés de professorat i regidors als cursets.
- **Base de dades**: MySQL 5.6 actualment, accedida només amb Entity Framework Core.

```
src/Web         API + Blazor
src/Apps/       apps MAUI (Cursets ara; espai per a futures apps)
db/             estructura de la base de dades (db/mysql/schema.sql)
docs/           documentació
tests/          proves automàtiques
```

No hi ha `src/Compartit`: l'app de Cursets no comparteix codi amb el Web, té les
seves pròpies classes de dades a `src/Apps/Cursets/Models`, mantingudes a mà com
a "mirall" dels DTO del Web (`src/Web/Models/Cursets/Dto`). En lloc d'una
llibreria compartida (que exigiria recompilar i tornar a publicar l'app, ja en
producció), `tests/AppAjuntament.Tests/ParitatDtosCursetsTests.cs` vigila per
codi font que els tipus no es desincronitzin. Si mai es refà l'app des de zero,
és el moment de reconsiderar-ho.

## Regles

Són inamovibles: una PR que no les compleixi no es fusiona.

1. **Connexions i proveïdors de BD.** Mai s'elimina ni es trenca un proveïdor existent. Suportar un altre motor sempre s'afegeix al costat dels que ja hi ha, amb el seu propi `db/<motor>/schema.sql`.
2. **Accés a dades només amb EF Core.** Cap SQL cru (`FromSqlRaw`, `ExecuteSqlRaw`), cap ADO.NET ni procediments emmagatzemats. Cap consulta pot dependre d'una funció exclusiva d'un motor.
3. **Secrets fora del repositori.** Cap contrasenya, clau, token ni cadena de connexió real al codi ni a fitxers versionats. Van a `appsettings.<Entorn>.secrets.json` (ignorat per git); al repositori només hi ha `appsettings.secrets.sample.json`, sense valors.
4. **Cap dada personal real** en codi, tests, exemples, documentació ni SQL. Es fan servir dades fictícies.
5. **Res d'infraestructura pròpia de l'Ajuntament**: ni desplegament, ni servidors, ni usuaris ni credencials.
6. **L'estructura de la BD viatja amb el model.** Tot canvi de model actualitza `db/*/schema.sql` en el mateix commit. No hi ha migracions. Els fitxers `.sql` estan ignorats per git, tret dels de `db/`, i inclouen només estructura i dades no personals.
7. **Tests obligatoris.** Tota funcionalitat nova o correcció d'un error porta el seu test, i tots els tests passen a cada PR.
8. **.NET 10 és la versió objectiu.** Només els responsables del repositori poden canviar-la.
9. **Revisió humana.** Tota PR l'aprova una persona responsable. Una eina d'IA mai aprova ni fusiona la seva pròpia feina.

## Convencions

- **Idioma:** català al codi, als comentaris, als commits i a la documentació.
- **Commits:** `tipus(àmbit): descripció` en imperatiu.
  - Tipus: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`.
  - Àmbits (llista tancada): `cursets`, `subvencions`, `patrimoni`, `armes`, `voluntaris`, `tercers`, `nav`, `app`, `bdd`, `docs`, `ci`, `legal`. Per a un mòdul nou cal afegir-lo aquí primer.
  - Si hi ha participat una IA: `Co-Authored-By` al final del missatge.
- **Codi:** cada servei té la seva interfície (`ICursetsSeguimentService`); els DTOs van a `Models/<Mòdul>/Dto`; comentaris XML a les interfícies públiques.
- **Consentiment en formularis públics:** si un formulari sense login demana dades personals (com la inscripció a cursets), el text de consentiment ha de parlar dels **serveis de l'Ajuntament en general** (amb enllaç a `/data-privacy`), mai només del servei concret del formulari — les dades d'una persona (`Tercer`) es comparteixen entre mòduls, no queden aïllades al formulari que les ha recollit.
- **Comprovació:** `dotnet build` i `dotnet test` han de passar abans de donar una feina per acabada.
- **Branques:** `main` és protegida (només PR); `develop` és on s'integra el treball.
