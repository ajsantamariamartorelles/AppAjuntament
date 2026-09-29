using AppAjuntament.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AppAjuntament.Tests.Infraestructura;

/// <summary>
/// Base de dades SQLite en memòria amb el model real de l'aplicació.
/// Cada instància és una BD nova i aïllada: els tests no comparteixen estat.
/// </summary>
public sealed class BdProves : IDisposable
{
    private readonly SqliteConnection _connexio;

    public BdProves()
    {
        _connexio = new SqliteConnection("DataSource=:memory:");
        _connexio.Open();

        // Les dades de cada test només porten el que necessita: no s'exigeixen
        // les claus foranes de taules alienes al que es prova.
        using (var cmd = _connexio.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }

        using var context = NouContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Un context nou sobre la mateixa BD (per separar l'escriptura de la lectura).</summary>
    public GestorSubvencionsContext NouContext()
    {
        var opcions = new DbContextOptionsBuilder<GestorSubvencionsContext>()
            .UseSqlite(_connexio)
            .Options;
        return new GestorSubvencionsContext(opcions);
    }

    public void Dispose() => _connexio.Dispose();
}
