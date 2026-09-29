using AjuntamentSantaMariaMartorelles.ViewModels;

namespace AjuntamentSantaMariaMartorelles.Views;

public partial class CursetSeguimentPage : ContentPage
{
	public CursetSeguimentPage(CursetSeguimentViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
