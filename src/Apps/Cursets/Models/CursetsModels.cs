namespace AjuntamentSantaMariaMartorelles.Models;

// Mirall dels DTOs del backend (AppAjuntament: Models/Cursets/Dto/CursetsDtos.cs).
// Només s'inclouen aquí els que fa servir el client de la professora.

public class LoginRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
	public string Token { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
	/// <summary>"Professora" o "Controlador" (regidor, només lectura).</summary>
	public string Rol { get; set; } = "Professora";
	public int ProfessoraId { get; set; }
	public string ProfessoraNom { get; set; } = string.Empty;
	/// <summary>Si cal mostrar la pantalla d'acceptació de termes i protecció de
	/// dades abans d'entrar (encara no ha acceptat la versió vigent).</summary>
	public bool CalAcceptarTermes { get; set; }
}

public class CursetDto
{
	public int Id { get; set; }
	public string Nom { get; set; } = string.Empty;
	public string Titol { get; set; } = string.Empty;
	public int TipusCursetId { get; set; }
	public string TipusCursetNom { get; set; } = string.Empty;
	public DayOfWeek? DiaSetmana { get; set; }
	public TimeSpan? HoraInici { get; set; }
	public TimeSpan? HoraFi { get; set; }
	public bool Actiu { get; set; }
	public int NumAlumnes { get; set; }
	public decimal PreuPerSessioEmpadronat { get; set; }
	public decimal PreuPerSessioNoEmpadronat { get; set; }
	/// <summary>Nombre màxim de places (null = sense límit).</summary>
	public int? MaxPlaces { get; set; }

	/// <summary>Només per al controlador: resum d'inscrites i pendents (crida a part, no ve amb el curset).</summary>
	public ResumSeguimentCursetDto? Seguiment { get; set; }

	public bool TeSeguiment => Seguiment is not null;

	/// <summary>Etiquetes de la targeta per al controlador: places, llista d'espera i pagaments.</summary>
	public IReadOnlyList<Pill> Etiquetes
	{
		get
		{
			if (Seguiment is not { } s)
				return Array.Empty<Pill>();

			var etiquetes = new List<Pill>
			{
				Pill.Neutre(MaxPlaces is int max ? $"{s.NumAdmeses} / {max} places" : $"{s.NumAdmeses} inscrites")
			};
			if (s.NumLlistaEspera > 0)
				etiquetes.Add(Pill.Avis($"{s.NumLlistaEspera} en espera"));

			if (s.NumPendents > 0)
				etiquetes.Add(Pill.Perill(s.NumPendents == 1 ? "1 pendent de pagar" : $"{s.NumPendents} pendents de pagar"));
			else if (s.TeLiquidacions)
				etiquetes.Add(Pill.Ok("Tot cobrat"));
			else
				etiquetes.Add(Pill.Neutre("Sense liquidar"));

			return etiquetes;
		}
	}

	/// <summary>Text llegible de l'horari setmanal (p.ex. "Dilluns 10:00 - 11:00").</summary>
	public string HorariText =>
		DiaSetmana is null || HoraInici is null || HoraFi is null
			? "Horari no definit"
			: $"{TextDia(DiaSetmana.Value)} {HoraInici:hh\\:mm} - {HoraFi:hh\\:mm}";

	private static string TextDia(DayOfWeek dia) => dia switch
	{
		DayOfWeek.Monday => "Dilluns",
		DayOfWeek.Tuesday => "Dimarts",
		DayOfWeek.Wednesday => "Dimecres",
		DayOfWeek.Thursday => "Dijous",
		DayOfWeek.Friday => "Divendres",
		DayOfWeek.Saturday => "Dissabte",
		DayOfWeek.Sunday => "Diumenge",
		_ => string.Empty
	};
}

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
