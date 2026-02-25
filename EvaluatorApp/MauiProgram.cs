using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EvaluatorApp.Services;

using CommunityToolkit.Maui;

namespace EvaluatorApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement(false)
            .ConfigureFonts(fonts =>
            {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");
			});

        var configBuilder = new ConfigurationBuilder();

        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            if (stream != null)
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                configBuilder.AddJsonStream(ms);
            }
        }
        catch { /* archivo no encontrado, continuar sin config base */ }

#if DEBUG
        try
        {
            using var devStream = FileSystem.OpenAppPackageFileAsync("appsettings.Development.json").GetAwaiter().GetResult();
            if (devStream != null)
            {
                var ms = new MemoryStream();
                devStream.CopyTo(ms);
                ms.Position = 0;
                configBuilder.AddJsonStream(ms);
            }
        }
        catch { /* archivo de desarrollo no encontrado */ }
#endif

        var config = configBuilder.Build();
        builder.Configuration.AddConfiguration(config);

        // Register API Service instead of direct MongoDB
        builder.Services.AddSingleton<Services.IMongoDBService, Services.ApiService>();
        builder.Services.AddSingleton<Services.LocalDbService>(); // Local DB Service
        builder.Services.AddSingleton<Services.ProjectRepository>(); // Repository
        builder.Services.AddSingleton<Services.VideoService>(); // Video Downloader
        builder.Services.AddSingleton<Services.ICloudinaryService, Services.CloudinaryService>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ProjectsPage>();
        builder.Services.AddSingleton<RankingPage>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddTransient<ProjectDetailsPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<EditProfilePage>();
        builder.Services.AddSingleton<Services.IMsalAuthService, Services.MsalAuthService>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
