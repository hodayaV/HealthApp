using HealthApp.Components.Data;
using Microsoft.Extensions.Logging;

namespace HealthApp
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddMauiBlazorWebView();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif
			builder.Services.AddSingleton<FoodService>();
			builder.Services.AddSingleton<IConnectivity>(c => Connectivity.Current);
			//builder.Services.AddSingleton<GoalService>();
			return builder.Build();
		}
	}
}
