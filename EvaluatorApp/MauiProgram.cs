using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EvaluatorApp.Services;

namespace EvaluatorApp;

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

                fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");
			});

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("EvaluatorApp.Resources.Raw.appsettings.json");

        var configBuilder = new ConfigurationBuilder()
            .AddJsonStream(stream);

#if DEBUG
        using var devStream = assembly.GetManifestResourceStream("EvaluatorApp.Resources.Raw.appsettings.Development.json");
        if (devStream != null)
        {
            configBuilder.AddJsonStream(devStream);
        }
#endif

        var config = configBuilder.Build();
        builder.Configuration.AddConfiguration(config);

        // Register API Service instead of direct MongoDB
        builder.Services.AddSingleton<Services.IMongoDBService, Services.ApiService>();
        // builder.Services.AddSingleton<Services.IMongoDBService, Services.MongoDBService>();
        builder.Services.AddSingleton<Services.ICloudinaryService, Services.CloudinaryService>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<ProjectsPage>();
        builder.Services.AddSingleton<RankingPage>();
        builder.Services.AddTransient<ProjectDetailsPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<EditProfilePage>();
        builder.Services.AddSingleton<Services.IMsalAuthService, Services.MsalAuthService>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
