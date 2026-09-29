using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class PersonaPage : ContentPage
{
	public PersonaPage(PersonaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
