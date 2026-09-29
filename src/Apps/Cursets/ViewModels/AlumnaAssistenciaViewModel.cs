using CommunityToolkit.Mvvm.ComponentModel;
using AjuntamentSantaMariaMartorelles.Models;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Embolcall observable d'una fila d'assistència: cal perquè el CheckBox de
/// "present" es pugui enllaçar en dues direccions des de la graella.
/// </summary>
public partial class AlumnaAssistenciaViewModel : ObservableObject
{
	public int AlumneId { get; }
	public string NomComplet { get; }

	[ObservableProperty]
	private bool present;

	public AlumnaAssistenciaViewModel(AssistenciaAlumneDto dto)
	{
		AlumneId = dto.AlumneId;
		NomComplet = dto.NomComplet;
		Present = dto.Present;
	}

	public AssistenciaAlumneDto ToDto() => new()
	{
		AlumneId = AlumneId,
		NomComplet = NomComplet,
		Present = Present
	};
}
