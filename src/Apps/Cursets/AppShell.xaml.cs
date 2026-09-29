using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Rutes que es naveguen amb push (fletxa enrere activa):
		// des del menú a la llista de cursets, i d'aquí a la sessió.
		Routing.RegisterRoute(nameof(CursetsListPage), typeof(CursetsListPage));
		Routing.RegisterRoute(nameof(SessioPage), typeof(SessioPage));
		Routing.RegisterRoute(nameof(CursetSeguimentPage), typeof(CursetSeguimentPage));
		Routing.RegisterRoute(nameof(PersonaPage), typeof(PersonaPage));
		Routing.RegisterRoute(nameof(CobramentsPage), typeof(CobramentsPage));
		Routing.RegisterRoute(nameof(ComptePage), typeof(ComptePage));
	}
}
