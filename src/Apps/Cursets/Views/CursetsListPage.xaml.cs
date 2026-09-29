using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class CursetsListPage : ContentPage
{
	private readonly CursetsListViewModel _viewModel;

	public CursetsListPage(CursetsListViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.CarregaSiCalAsync();
	}
}
