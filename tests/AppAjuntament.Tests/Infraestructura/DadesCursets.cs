using AppAjuntament.Models;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Tercers;

namespace AppAjuntament.Tests.Infraestructura;

/// <summary>Constructors de dades fictícies de Cursets. Cap dada real.</summary>
public static class DadesCursets
{
    public static Curset Curset(GestorSubvencionsContext bd, string nom, int? regidorId,
        DateTime? inscripcioInici = null, DateTime? inscripcioFi = null,
        decimal preuEmpadronat = 2m, decimal preuNoEmpadronat = 3m, bool actiu = true)
    {
        var curset = new Curset
        {
            Nom = nom,
            TipusCurset = bd.TipusCursets.FirstOrDefault(t => t.Nom == "Pilates")
                          ?? new TipusCurset { Nom = "Pilates" },
            RegidorId = regidorId,
            Actiu = actiu,
            InscripcioInici = inscripcioInici,
            InscripcioFi = inscripcioFi,
            PreuPerSessioEmpadronat = preuEmpadronat,
            PreuPerSessioNoEmpadronat = preuNoEmpadronat
        };
        bd.Cursets.Add(curset);
        bd.SaveChanges();
        return curset;
    }

    public static Alumne Alumne(GestorSubvencionsContext bd, string nom, string? cognoms = null,
        bool empadronat = true)
    {
        var alumne = new Alumne
        {
            Empadronat = empadronat,
            Tercer = new Tercer { Nom = nom, Cognoms = cognoms }
        };
        bd.CursetsAlumnes.Add(alumne);
        bd.SaveChanges();
        return alumne;
    }

    public static AlumneCurset Inscripcio(GestorSubvencionsContext bd, Curset curset, Alumne alumne,
        EstatInscripcio estat, int? ordreLlistaEspera = null, DateTime? dataAlta = null,
        DateTime? dataBaixa = null)
    {
        var inscripcio = new AlumneCurset
        {
            CursetId = curset.Id,
            AlumneId = alumne.Id,
            Estat = estat,
            OrdreLlistaEspera = ordreLlistaEspera,
            DataAlta = dataAlta ?? DateTime.Today.AddMonths(-3),
            DataBaixa = dataBaixa
        };
        bd.AlumnesCursets.Add(inscripcio);
        bd.SaveChanges();
        return inscripcio;
    }

    public static Liquidacio Liquidacio(GestorSubvencionsContext bd, Curset curset, Alumne alumne,
        decimal import, EstatLiquidacio estat = EstatLiquidacio.Emesa, DateTime? dataEmissio = null,
        string periode = "T1")
    {
        var liquidacio = new Liquidacio
        {
            Numero = Guid.NewGuid().ToString("N")[..12],
            CursetId = curset.Id,
            AlumneId = alumne.Id,
            PeriodeEtiqueta = periode,
            NumSessions = 10,
            PreuPerSessio = import / 10m,
            Import = import,
            Estat = estat,
            DataEmissio = dataEmissio ?? DateTime.Today.AddDays(-10)
        };
        bd.CursetsLiquidacions.Add(liquidacio);
        bd.SaveChanges();
        return liquidacio;
    }

    public static Sessio Sessio(GestorSubvencionsContext bd, Curset curset, DateTime data,
        EstatSessio estat = EstatSessio.Tancada, params (Alumne alumne, bool present)[] assistencies)
    {
        var sessio = new Sessio { CursetId = curset.Id, Data = data, Estat = estat };
        foreach (var (alumne, present) in assistencies)
            sessio.Assistencies.Add(new Assistencia { AlumneId = alumne.Id, Present = present });
        bd.CursetsSessions.Add(sessio);
        bd.SaveChanges();
        return sessio;
    }
}
