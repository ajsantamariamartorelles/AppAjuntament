namespace AjuntamentSantaMariaMartorelles;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// L'app arrenca a AppShell → ruta "loading", que comprova si hi ha
		// sessió guardada i salta a "//menu" o "//login".
		MainPage = new AppShell();
	}
}
