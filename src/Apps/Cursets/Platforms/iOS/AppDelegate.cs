using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace AjuntamentSantaMariaMartorelles;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	// Reenvia a MSAL la redirecció d'Azure AD (msauth.cat.santamariademartorelles.mobil://auth).
	public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
	{
		AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
		return true;
	}
}
