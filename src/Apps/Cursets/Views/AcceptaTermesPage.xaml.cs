using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class AcceptaTermesPage : ContentPage
{
	public AcceptaTermesPage(AcceptaTermesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
