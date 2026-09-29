using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class ComptePage : ContentPage
{
	private readonly CompteViewModel _viewModel;

	public ComptePage(CompteViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.Actualitza();
	}
}
