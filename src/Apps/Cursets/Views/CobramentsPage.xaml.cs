using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class CobramentsPage : ContentPage
{
	private readonly CobramentsViewModel _viewModel;

	public CobramentsPage(CobramentsViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (_viewModel.Grups.Count == 0)
			await _viewModel.CarregarAsync();
	}
}
