using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StudentApp;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("StudentApp.Resources.Raw.appsettings.json");
        
        var configBuilder = new ConfigurationBuilder()
            .AddJsonStream(stream);

#if DEBUG
        using var devStream = assembly.GetManifestResourceStream("StudentApp.Resources.Raw.appsettings.Development.json");
        if (devStream != null)
        {
            configBuilder.AddJsonStream(devStream);
        }
#endif

        var config = configBuilder.Build();
        builder.Configuration.AddConfiguration(config);

        builder.Services.AddSingleton<Services.IMongoDBService, Services.MongoDBService>();
        builder.Services.AddSingleton<Services.IMsalAuthService, Services.MsalAuthService>();
        
        // Register Pages
        builder.Services.AddTransient<Views.LoadingPage>();
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Views.TasksPage>();
        builder.Services.AddTransient<Views.MessagesPage>();
        builder.Services.AddTransient<Views.ProfilePage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
