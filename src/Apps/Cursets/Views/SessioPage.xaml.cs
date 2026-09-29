using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class SessioPage : ContentPage
{
	public SessioPage(SessioViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
