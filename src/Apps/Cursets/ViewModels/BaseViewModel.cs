using CommunityToolkit.Mvvm.ComponentModel;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

public partial class BaseViewModel : ObservableObject
{
	[ObservableProperty]
	private bool isBusy;

	[ObservableProperty]
	private string? errorMessage;

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));
}
